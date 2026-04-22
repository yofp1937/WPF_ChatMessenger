/*
 * Client에서 친구 관련 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
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
                .Where(f => f.UserEmail == myEmail && !f.IsBlocked)        // 1.Friendships 테이블에서 UserEmail과 myEmail 값이 같은것과 IsBlocked가 false인것들 필터링
                .Join(_context.Users,                                                    // 2.Users와 연결
                    friendship => friendship.FriendEmail,                          // 3.Friendship.FriendEmail과 user.Email이 일치하는 데이터들을 꺼내겠다
                    user => user.Email,
                    (friendship, user) => new FriendResponse                    // 4.두 테이블에서 값을 꺼내서 새로운 FriendResponse를 만들겠다.
                    {
                        Email = user.Email,
                        Nickname = user.Nickname,
                        StatusMessage = user.StatusMessage,
                        ProfileImageURL = user.ProfileImageURL,
                        IsAdded = true,
                        IsBlocked = friendship.IsBlocked,
                        IsFavorite = friendship.IsFavorite,
                    })
                .ToListAsync();

            return Ok(friendList);
        }

        /// <summary>
        /// 친구 검색 요청을 처리합니다.
        /// </summary>
        /// <param name="email">검색하려는 친구의 이메일</param>
        [HttpGet("searchuser")]
        public async Task<ActionResult<FriendResponse>> SearchUser([FromQuery]string email)
        {
            // 1.토큰에서 내 이메일 추출
            string? myEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(myEmail)) return Unauthorized();

            // 2.유저 검색
            User? targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (targetUser == null) return NotFound("사용자를 찾을 수 없습니다.");

            // 3.검색된 유저와 나의 관계 정보 확인
            Friendship? friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.UserEmail == myEmail && f.FriendEmail == email);

            // 4.반환용 FriendResponse 객체 생성
            FriendResponse response = new()
            {
                Email = targetUser.Email,
                Nickname = targetUser.Nickname,
                StatusMessage = targetUser.StatusMessage,
                ProfileImageURL = targetUser.ProfileImageURL,
                // 관계 데이터가 존재하면 넣고 없으면 false
                IsAdded = friendship != null,
                IsBlocked = friendship?.IsBlocked ?? false,
                IsFavorite = friendship?.IsFavorite ?? false,
            };
            return Ok(response);
        }
    }
}