/*
 * Client에서 친구 관련 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Controllers.Base;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Requests.Friend;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using Microsoft.AspNetCore.Mvc;

namespace ChatMessenger.Server.Controllers
{
    [Route("api/[controller]")] // 주소: "https://서버주소/api/friend" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class FriendController : AuthorizedBaseController
    {
        private readonly ISocialService _friendService;
        public FriendController(ISocialService friendService) : base()
        {
            _friendService = friendService;
        }

        #region public Method
        /// <summary>
        /// 친구 목록 요청을 처리합니다.
        /// </summary>
        [HttpGet("getlist")]
        public async Task<IActionResult> GetFriendList()
        {
            // 1. Service를 통해 친구 List 요청
            ServiceResult<List<FriendResponse>> friendList = await _friendService.GetFriendResponseListAsync(CurrentUserEmail);
            return ContextResponse(friendList);
        }
        /// <summary>
        /// 친구 추가 요청을 처리합니다.
        /// </summary>
        /// <param name="request">친구 목록에 추가하려는 친구의 이메일</param>
        [HttpPost("addfriend")]
        public async Task<IActionResult> AddFriend([FromBody] AddorDeleteFriendRequest request)
        {
            // 1. Service를 통해 친구 추가 요청
            ServiceResult<FriendResponse> response = await _friendService.AddFriendAsync(CurrentUserEmail, request.Email);
            return ContextResponse(response);
        }
        /// <summary>
        /// 친구 삭제 요청을 처리합니다.
        /// </summary>
        /// <param name="request">삭제하려는 친구의 이메일</param>
        [HttpPost("deletefriend")]
        public async Task<IActionResult> DeleteFriend([FromBody] AddorDeleteFriendRequest request)
        {
            // 1. 친구 삭제 요청
            ServiceResult<bool> response = await _friendService.DeleteFriendAsync(CurrentUserEmail, request.Email);
            return ContextResponse(response);
        }
        /// <summary>
        /// 즐겨찾기 상태 변경을 처리합니다.
        /// </summary>
        /// <param name="request">친구의 이메일과 변경하려는 즐겨찾기 상태(true인지, false인지)</param>
        [HttpPatch("changefavorite")]
        public async Task<IActionResult> UpdateFavorite([FromBody] FriendStatusRequest request)
        {
            // 1. 즐겨찾기 변경 요청
            ServiceResult<bool> response = await _friendService.UpdateFavoriteAsync(CurrentUserEmail, request);
            return ContextResponse(response);
        }
        /// <summary>
        /// 차단 상태 변경을 처리합니다.
        /// </summary>
        /// <param name="request">친구의 이메일과 변경하려는 차단 상태(true인지, false인지)</param>
        [HttpPatch("changeblock")]
        public async Task<IActionResult> UpdateBlock([FromBody] FriendStatusRequest request)
        {
            // 1. 차단 변경 요청
            ServiceResult<bool> response = await _friendService.UpdateBlockAsync(CurrentUserEmail, request);
            return ContextResponse(response);
        }
        /// <summary>
        /// 친구 검색 요청을 처리합니다.
        /// </summary>
        /// <param name="friendEmail">검색하려는 친구의 이메일</param>
        [HttpGet("searchuser")]
        public async Task<IActionResult> SearchUser([FromQuery] string friendEmail)
        {
            // 1. 유저 검색
            ServiceResult<FriendResponse> response = await _friendService.GetFriendResponseAsync(friendEmail);
            return ContextResponse(response);
        }
        #endregion
    }
}