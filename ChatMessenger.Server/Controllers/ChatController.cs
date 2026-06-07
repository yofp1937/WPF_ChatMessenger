/*
 * Client에서 채팅방 목록 요청, 채팅방 상세 내용 요청 등 채팅과 관련된 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Controllers.Base;
using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Hubs;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Constants;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatMessenger.Server.Controllers
{
    [Route("api/[controller]")] // 주소: "https://서버주소/api/chat" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class ChatController : AuthorizedBaseController
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        #region 채팅방 관련 메서드
        /// <summary>
        /// 특정 채팅방의 간략한 정보가 담긴 ChatRoomSummaryResponse 객체를 반환해줍니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        [HttpGet("getchatroom/{roomId}")]
        public async Task<IActionResult> GetChatRoomSummaryResponseAsync(Guid roomId)
        {
            // 1. service에게 채팅방 ChatRoomSummaryResponse 요청
            ServiceResult<ChatRoomSummaryResponse> response = await _chatService.GetChatRoomSummaryResponseAsync(roomId, CurrentUserEmail);
            // 2. 결과 반환
            return ContextResponse(response);
        }
        /// <summary>
        /// 로그인한 사용자가 참여한 모든 채팅방의 간략한 정보가 담긴 ChatRoomSummaryResponse List를 반환해줍니다.
        /// </summary>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        [HttpGet("getchatroomlist")]
        public async Task<IActionResult> GetChatRoomSummaryResponseListAsync()
        {
            // 1. service에게 내가 가입한 채팅방들의 ChatRoomSummaryResponse 목록 요청
            ServiceResult<List<ChatRoomSummaryResponse>> response = await _chatService.GetChatRoomSummaryResponseListAsync(CurrentUserEmail);
            // 2. 결과 반환
            return ContextResponse(response);
        }
        /// <summary>
        /// 특정 채팅방의 상세한 정보가 담긴 ChatRoomDetailResponse 객체를 반환해줍니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        [HttpGet("join/{roomId}")]
        public async Task<IActionResult> GetChatRoomDetailAsync(Guid roomId)
        {
            // 1. service에게 채팅방의 ChatRoomDetailResponse 요청
            ServiceResult<ChatRoomDetailResponse> response = await _chatService.GetChatRoomDetailResponseAsync(roomId, CurrentUserEmail);
            // 2. 결과 반환
            return ContextResponse(response);
        }
        /// 그룹 채팅방을 생성합니다.
        /// </summary>
        /// <param name="request">채팅방 생성 정보</param>
        [HttpPost("creategroupchat")]
        public async Task<IActionResult> CreateGroupChatAsync([FromBody] CreateGroupChatRequest request)
        {
            ServiceResult<Guid> response = await _chatService.CreateGroupChatRoomAsync(CurrentUserEmail, request);;
            return ContextResponse(response);
        }
        /// <summary>
        /// 1대1 채팅방이 존재하는지 확인하고 없으면 생성하여 반환해줍니다.
        /// </summary>
        /// <param name="request">1대1 채팅방 생성에 필요한 정보 DTO</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        [HttpPost("getprivate")]
        public async Task<IActionResult> GetOrCreatePrivateChatAsync([FromBody] CreatePrivateChatRequest request)
        {
            ServiceResult<Guid> response = await _chatService.GetOrCreatePrivateChatAsync(CurrentUserEmail, request);
            return ContextResponse(response);
        }
        #endregion 채팅방 관련 메서드
        #region 참가자 관련 메서드
        /// <summary>
        /// 특정 채팅방에서 나갑니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        [HttpPost("leave/{roomId}")]
        public async Task<IActionResult> LeaveRoomAsync(Guid roomId)
        {
            ServiceResult<bool> response = await _chatService.RemoveParticipantAndCreateLeaveMessageAsync(roomId, CurrentUserEmail);
            return ContextResponse(response);
        }
        /// <summary>
        /// 채팅방에 다른 참가자들을 초대합니다.
        /// </summary>
        /// <param name="request">초대할 방과 참가자들에 대한 정보</param>
        [HttpPost("invite")]
        public async Task<IActionResult> InviteParticipantsAsync([FromBody] InviteParticipantsRequest request)
        {
            ServiceResult<bool> response = await _chatService.AddParticipantsToRoomAsync(request.RoomId, CurrentUserEmail, request.ParticipantEmails);
            return ContextResponse(response);
        }
        #endregion 참가자 관련 메서드
        #region 메세지 관련 메서드
        /// <summary>
        /// 사용자가 특정 메세지를 읽었을때 실행됩니다.<br/>
        /// Db 업데이트를 실행하고, 다른 참가자들에게 View 업데이트 요청을 전송합니다.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("readmessage")]
        public async Task<IActionResult> UpdateLastReadedMessageAsync([FromBody] UpdateLastReadedMessageRequest request)
        {
            // 1. service에게 마지막으로 읽은 메세지 갱신 요청하고, 결과 받아옴
            ServiceResult<UserReadUpdateResponse> result = await _chatService.UpdateLastReadedMessageAsync(CurrentUserEmail, request);
            if (!result.IsSuccess)
                return ContextResponse(ServiceResult<bool>.Failed(result.ErrorMessage, result.ResultType));
            // 2. 결과 반환
            return ContextResponse(ServiceResult<bool>.Success(true));
        }
        /// <summary>
        /// 특정 채팅방에 메세지를 전송합니다.
        /// </summary>
        /// <param name="request">메세지 정보가 담긴 Request DTO</param>
        [HttpPost("sendmessage")]
        public async Task<IActionResult> SendMessageAsync([FromBody] SendMessageRequest request)
        {
            // 1. 메세지 Db에 등록하고 채팅 참가자들에게 전송하기위해 Response로 반환받음
            ServiceResult<ChatMessageResponse> messageResponse = await _chatService.SendMessageAsync(CurrentUserEmail, request);
            if (!messageResponse.IsSuccess)
                return ContextResponse(ServiceResult<bool>.Failed(messageResponse.ErrorMessage, messageResponse.ResultType));
            // 2. 결과 반환
            return ContextResponse(ServiceResult<bool>.Success(true));
        }
        #endregion 메세지 관련 메서드
    }
}
