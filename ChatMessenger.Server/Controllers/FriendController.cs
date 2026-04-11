/*
 * Client에서 친구 관련 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Data;
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
                .Where(f => f.UserEmail == myEmail)                             // 1.Friendships 테이블에서 UserEmail과 myEmail 값이 같은것만 필터링
                .Join(_context.Users,                                                    // 2.Users 테이블에서 데이터를 꺼내겠다
                    friendship => friendship.FriendEmail,                          // 3.Friendship.FriendEmail과 user.Email이 일치하는 데이터들을 꺼내겠다
                    user => user.Email,                                                 
                    (friendship, user) => new FriendResponse                    // 4.두 테이블에서 값을 꺼내서 새로운 FriendResponse를 만들겠다.
                    {
                        Email = user.Email,
                        Nickname = user.Nickname,
                        StatusMessage = user.StatusMessage,
                        ProfileImageURL = user.ProfileImageURL,
                    })
                .ToListAsync();

            return Ok(friendList);
        }
    }
}