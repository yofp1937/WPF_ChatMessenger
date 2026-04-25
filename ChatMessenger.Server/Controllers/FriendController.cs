/*
 * Client에서 친구 관련 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Extensions;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Controllers
{
    [Authorize]
    [ApiController] // 웹 API 응답을 전담하는 Controller임을 선언
    [Route("api/[controller]")] // 주소: "https://서버주소/api/friend" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class FriendController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FriendController(AppDbContext context)
        {
            _context = context;
        }

        #region public Method
        /// <summary>
        /// 친구 목록 요청을 처리합니다.
        /// </summary>
        [HttpGet("list")]
        public async Task<ActionResult<List<FriendResponse>>> GetFriends()
        {
            // 토큰에서 친구 목록을 요청한 User의 Email 추출
            string? myEmail = User.Identity?.Name;
            var test = User.Identity;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();

            // 내 친구 목록(Friendships)을 가져와서 User 테이블과 Join하여 상세 정보 추출
            List<FriendResponse> friendList = await _context.Friendships
                // 1. Friendship 테이블에서 기본적으로 아래 조건을 충족하는 데이터를 사용
                .Where(f => f.UserEmail == myEmail && !f.IsBlocked)
                // 2. User 테이블과 Join하여 FriendResponse 구조로 데이터를 추출할 Sql 쿼리 생성
                .ProjectToFriendResponse(_context.Users)
                // 3. Sql을 Db에서 실행하고 결과 데이터를 FriendResponse 객체 리스트로 반환
                .ToListAsync();

            Console.WriteLine($"[{DateTime.Now}][Friend/list]: {myEmail}님이 친구 목록을 요청하셨습니다.");
            return Ok(friendList);
        }

        /// <summary>
        /// 친구 검색 요청을 처리합니다.
        /// </summary>
        /// <param name="friendEmail">검색하려는 친구의 이메일</param>
        [HttpGet("searchuser")]
        public async Task<ActionResult<FriendResponse>> SearchUser([FromQuery] string friendEmail)
        {
            // 1.토큰에서 내 이메일 추출
            string? myEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();

            // 2.유저 검색
            User? targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == friendEmail);
            if (targetUser == null) return NotFound("사용자를 찾을 수 없습니다.");

            // 3.검색된 유저와 나의 관계 정보 확인
            Friendship? friendship = await GetFriendshipAsync(myEmail, friendEmail);
            bool isMe = targetUser.Email == myEmail;

            // 4.반환용 FriendResponse 객체 생성
            FriendResponse response = targetUser.MapToFriendResponse(friendship, isMe);

            Console.WriteLine($"[{DateTime.Now}][Friend/searchuser]: {myEmail}님이 {friendEmail}님을 검색하셨습니다.");
            return Ok(response);
        }

        /// <summary>
        /// 친구 추가 요청을 처리합니다.
        /// </summary>
        /// <param name="request">친구 목록에 추가하려는 친구의 이메일</param>
        [HttpPost("add")]
        public async Task<ActionResult<FriendResponse>> AddFriend([FromBody] AddorDeleteFriendRequest request)
        {
            // 1.토큰에서 내 이메일 추출
            string? myEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();
            // 2.Body에서 친구 이메일 추출
            string friendEmail = request.Email;

            if (myEmail == friendEmail) return BadRequest("자기 자신은 친구로 추가할 수 없습니다.");

            // 2.기존 관계 확인 (이미 친구거나 차단 상태인지)
            Friendship? friendship = await GetFriendshipAsync(myEmail, friendEmail);

            if (friendship != null)
            {
                if (friendship.IsBlocked) return BadRequest("차단된 사용자입니다. 차단 해제 후 추가해주세요.");
                return BadRequest("이미 추가된 사용자입니다.");
            }

            // 3.친구 등록
            Friendship newFriendship = new()
            {
                UserEmail = myEmail,
                FriendEmail = friendEmail,
                IsBlocked = false,
                IsFavorite = false
            };
            _context.Friendships.Add(newFriendship);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{DateTime.Now}][Friend/add]: {myEmail}님이 {friendEmail}님을 친구로 등록하셨습니다.");
            // 4.등록된 친구의 정보를 넘겨줘야하는데 SearchUser 재활용해서 넘겨줌
            return await SearchUser(friendEmail);
        }

        /// <summary>
        /// 친구 삭제 요청을 처리합니다.
        /// </summary>
        /// <param name="request">삭제하려는 친구의 이메일</param>
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteFriend([FromBody] AddorDeleteFriendRequest request)
        {
            // 1.토큰에서 내 이메일 추출
            string? myEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();
            // 2.Body에서 친구 이메일 추출
            string friendEmail = request.Email;

            if (myEmail == friendEmail) return BadRequest("자기 자신은 삭제할 수 없습니다.");

            // 3.기존 관계 확인 (이미 친구인지)
            // 차단 상태인건 삭제하면 안되니 걸러야함
            Friendship? friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserEmail == myEmail && f.FriendEmail == friendEmail && f.IsBlocked == false);
            if (friendship == null) return BadRequest("친구 등록이 되어있지않은 사용자입니다.");

            // 4.친구 삭제
            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{DateTime.Now}][Friend/delete]: {myEmail}님이 {friendEmail}님을 친구 삭제하셨습니다.");
            return Ok();
        }

        /// <summary>
        /// 즐겨찾기 상태 변경을 처리합니다.
        /// </summary>
        /// <param name="request">친구의 이메일과 변경하려는 즐겨찾기 상태(true인지, false인지)</param>
        [HttpPatch("favorite")]
        public async Task<IActionResult> UpdateFavorite([FromBody] FriendStatusRequest request)
        {
            // 1.토큰에서 내 이메일 추출
            string? myEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();
            // 2.Body에서 친구 이메일 추출
            string friendEmail = request.Email;

            // 3.기존 관계 확인 (친구로 등록돼있는지)
            Friendship? friendship = await GetFriendshipAsync(myEmail, friendEmail);
            if (friendship == null) return NotFound("친구 관계가 아닙니다.");

            // 4.등록돼있으면 IsFavorite 변경
            friendship.IsFavorite = request.IsFavorite;
            await _context.SaveChangesAsync();

            Console.WriteLine($"[{DateTime.Now}][Friend/favorite]: {myEmail}님이 {friendEmail}님의 즐겨찾기를 {request.IsFavorite} 상태로 변경하셨습니다.");
            return Ok();
        }

        /// <summary>
        /// 차단 상태 변경을 처리합니다.
        /// </summary>
        /// <param name="request">친구의 이메일과 변경하려는 차단 상태(true인지, false인지)</param>
        [HttpPatch("block")]
        public async Task<IActionResult> UpdateBlock([FromBody] FriendStatusRequest request)
        {
            // 1.토큰에서 내 이메일 추출
            string? myEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();
            // 2.Body에서 친구 이메일 추출
            string friendEmail = request.Email;

            // 3.기존 관계 확인 (친구로 등록돼있는지)
            Friendship? friendship = await GetFriendshipAsync(myEmail, friendEmail);

            // 4.차단하려는 경우
            if (request.IsBlocked)
            {
                // 등록된 관계가 없으면 차단 관계 생성
                if (friendship == null)
                {
                    _context.Friendships.Add(new Friendship
                    {
                        UserEmail = myEmail,
                        FriendEmail = friendEmail,
                        IsBlocked = true,
                        IsFavorite = false
                    });
                }
                // 친구 추가된 상대를 차단할땐 상태만 변경
                else
                {
                    friendship.IsBlocked = true;
                    friendship.IsFavorite = false;
                }
                Console.WriteLine($"[{DateTime.Now}][Friend/block]: {myEmail}님이 {friendEmail}님을 차단하셨습니다.");
            }
            // 5.차단 해제의 경우
            else
            {
                if (friendship == null) return BadRequest("차단 돼있지않은 상대입니다.");
                // 튜플 삭제하여 친구 추가부터 다시하게 만듦
                _context.Friendships.Remove(friendship);
                Console.WriteLine($"[{DateTime.Now}][Friend/block]: {myEmail}님이 {friendEmail}님의 차단을 해제하셨습니다.");
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
        #endregion
        #region private Method
        private async Task<Friendship?> GetFriendshipAsync(string myEmail, string friendEmail)
        {
            return await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserEmail == myEmail && f.FriendEmail == friendEmail);
        }
        #endregion
    }
}