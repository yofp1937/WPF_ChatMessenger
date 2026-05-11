/*
 * Client에서 친구 관련 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Controllers.Base;
using ChatMessenger.Server.Interfaces.Friend;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;

namespace ChatMessenger.Server.Controllers
{
    [Route("api/[controller]")] // 주소: "https://서버주소/api/friend" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class FriendController : AuthorizedBaseController
    {
        private readonly IFriendService _friendService;
        public FriendController(IFriendService friendService) : base()
        {
            _friendService = friendService;
        }

        #region public Method
        /// <summary>
        /// 친구 목록 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 친구 목록을 불러온 후 HTTP 응답 코드 200(OK)과 함께 친구 목록 반환
        /// </remarks>
        [HttpGet("getlist")]
        public async Task<ActionResult<List<FriendResponse>>> GetFriendList()
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. 내 이메일에 등록된 친구 목록을 가져옴
            List<FriendResponse> friendList = await _friendService.GetFriendListAsync(CurrentUserEmail);
            // 2. OK 신호와 함께 넘겨줌
            return Ok(friendList);
        }
        /// <summary>
        /// 친구 검색 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 검색 성공 시: HTTP 응답 코드 200(OK)과 함께 프로필 정보 반환<br/>
        /// 검색 실패 시: HTTP 응답 코드 404(NotFound)와 함께 에러 메세지 반환
        /// </remarks>
        /// <param name="friendEmail">검색하려는 친구의 이메일</param>
        [HttpGet("searchuser")]
        public async Task<ActionResult<FriendResponse>> SearchUser([FromQuery] string friendEmail)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. 유저 검색
            FriendResponse? response = await _friendService.SearchUserAsync(CurrentUserEmail, friendEmail);
            if (response == null) return NotFound("해당 이메일을 사용하는 사용자를 찾을 수 없습니다.");
            // 2. 유저가 존재하면 OK 신호와 함께 넘겨줌
            return Ok(response);
        }
        /// <summary>
        /// 친구 추가 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 친구 추가 성공 시: HTTP 응답 코드 200(OK)과 함께 프로필 정보 반환<br/>
        /// </remarks>
        /// <param name="request">친구 목록에 추가하려는 친구의 이메일</param>
        [HttpPost("addfriend")]
        public async Task<ActionResult<FriendResponse>> AddFriend([FromBody] AddorDeleteFriendRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();
            if (CurrentUserEmail == request.Email) return BadRequest("자기 자신은 친구로 추가할 수 없습니다.");

            // 1. 친구 추가 진행
            FriendResponse? response = await _friendService.AddFriendAsync(CurrentUserEmail, request.Email);
            if (response == null) return BadRequest("이미 등록됐거나 차단된 사용자입니다.");
            // 2. 친구 추가 완료됐으면 OK 신호와 함께 넘겨줌
            return Ok(response);
        }

        /// <summary>
        /// 친구 삭제 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 친구 삭제 성공 시: HTTP 응답 코드 200(OK) 반환<br/>
        /// 친구 삭제 실패 시: HTTP 응답 코드 400(BadRequest)과 함께 에러 메세지 반환
        /// </remarks>
        /// <param name="request">삭제하려는 친구의 이메일</param>
        [HttpPost("deletefriend")]
        public async Task<IActionResult> DeleteFriend([FromBody] AddorDeleteFriendRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();
            if (CurrentUserEmail == request.Email) return BadRequest("자기 자신은 삭제할 수 없습니다.");

            // 1. 친구 삭제 요청하고 반환
            bool response = await _friendService.DeleteFriendAsync(CurrentUserEmail, request.Email);
            return response ? Ok() : BadRequest("친구로 등록되지 않은 사용자입니다.");
        }

        /// <summary>
        /// 즐겨찾기 상태 변경을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 상태 변경 성공 시: HTTP 응답 코드 200(OK) 반환<br/>
        /// 상태 변경 실패 시: HTTP 응답 코드 400(BadRequest)과 함께 에러 메세지 반환
        /// </remarks>
        /// <param name="request">친구의 이메일과 변경하려는 즐겨찾기 상태(true인지, false인지)</param>
        [HttpPatch("changefavorite")]
        public async Task<IActionResult> UpdateFavorite([FromBody] FriendStatusRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. 즐겨찾기 변경 요청하고 반환
            bool response = await _friendService.UpdateFavoriteAsync(CurrentUserEmail, request);
            return response ? Ok() : BadRequest("대상과 친구 관계가 아닙니다.");
        }

        /// <summary>
        /// 차단 상태 변경을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 상태 변경 성공 시: HTTP 응답 코드 200(OK) 반환<br/>
        /// 상태 변경 실패 시: HTTP 응답 코드 400(BadRequest)과 함께 에러 메세지 반환
        /// </remarks>
        /// <param name="request">친구의 이메일과 변경하려는 차단 상태(true인지, false인지)</param>
        [HttpPatch("changeblock")]
        public async Task<IActionResult> UpdateBlock([FromBody] FriendStatusRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. 차단 변경 요청하고 반환
            bool response = await _friendService.UpdateBlockAsync(CurrentUserEmail, request);
            return response ? Ok() : BadRequest("처리할 수 없는 차단 요청입니다.");
        }
        #endregion
    }
}