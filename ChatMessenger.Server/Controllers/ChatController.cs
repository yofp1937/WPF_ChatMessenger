/*
 * Client에서 채팅방 목록 요청, 채팅방 상세 내용 요청 등 채팅과 관련된 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Controllers.Base;
using ChatMessenger.Server.Hubs;
using ChatMessenger.Server.Interfaces.Chat;
using ChatMessenger.Shared.Constants;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatMessenger.Server.Controllers
{
    [Route("api/[controller]")] // 주소: "https://서버주소/api/chat" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class ChatController : AuthorizedBaseController
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> chatHubContext)
        {
            _chatService = chatService;
            _chatHubContext = chatHubContext;
        }

        #region public Method
        /// <summary>
        /// 로그인한 사용자가 참여한 모든 채팅방 목록을 반환해줍니다.
        /// </summary>
        /// <returns>사용자가 참여한 모든 채팅방 데이터</returns>
        [HttpGet("getchatroomlist")]
        public async Task<IActionResult> GetMyChatRooms()
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. service에게 채팅방 목록 요청
            List<ChatRoomSummaryResponse> response = await _chatService.GetChatRoomListAsync(CurrentUserEmail);
            // 2. 결과 반환
            return Ok(response);
        }

        /// <summary>
        /// 1대1 채팅방이 존재하는지 확인하고 반환해줍니다.<br/>
        /// 없으면 채팅방을 생성하여 반환해줍니다.
        /// </summary>
        /// <param name="request">Client측에서 보내준 정보 묶음</param>
        /// <returns>찾았거나 생성한 방의 식별 번호</returns>
        [HttpPost("searchorcreateroom")]
        public async Task<IActionResult> CreatePrivateChatRoom([FromBody] CreatePrivateChatRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();
            string? targetEmail = request.TargetEmail;
            if (string.IsNullOrEmpty(targetEmail)) return BadRequest("대상이 존재하지 않습니다.");

            // 1. 방을 찾아서 반환(없으면 생성)
            Guid roomId = await _chatService.SearchOrCreatePrivateChatRoomAsync(CurrentUserEmail, targetEmail);
            return Ok(roomId);
        }

        /// <summary>
        /// 특정 채팅방의 상세 정보를 반환해줍니다.
        /// </summary>
        /// <param name="roomId">조회할 채팅방의 식별 번호</param>
        /// <returns>채팅방의 상세 정보(참가자, 메세지 내역 등)</returns>
        [HttpGet("{roomId}")]
        public async Task<IActionResult> GetChatRoomDetailAsync(Guid roomId)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. 채팅방 상세 데이터 조회 (내부에서 보안 검사도 실시)
            ChatRoomDetailResponse? response = await _chatService.GetChatRoomDetailAsync(roomId, CurrentUserEmail);
            if (response == null) return NotFound("채팅방을 찾을 수 없거나 접근 권한이 없습니다.");

            // 2. 존재하면 OK 신호와 함께 반환
            return Ok(response);
        }

        /// <summary>
        /// 특정 채팅방에 메세지를 전송합니다.
        /// </summary>
        /// <param name="request">메세지 정보가 담긴 Request DTO</param>
        [HttpPost("sendmessage")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            // 1. 메세지 Db에 등록하고 친구에게 전송하기위해 Response로 반환받음
            ChatMessageResponse? messageResponse = await _chatService.SendMessageAsync(CurrentUserEmail, request);
            if (messageResponse == null) return BadRequest("메세지를 전송하지 못했습니다.");

            // 2. 메세지 전송을 위해 채팅방의 모든 참가자 Email List 요청
            List<string>? participantEmails = await _chatService.GetParticipantEmailsAsync(CurrentUserEmail, request.RoomId);
            // 3. 해당방의 모든 참여자에게 메세지 전송
            if (participantEmails == null) return BadRequest("채팅방 참여자 정보를 받아오지못했습니다.");
            foreach (string email in participantEmails)
            {
                await _chatHubContext.Clients.Group(email)
                    .SendAsync(ChatHubEvents.ChatHubResponseEvent.ReceiveMessage, messageResponse);
            }
            return Ok();
        }

        /// <summary>
        /// 채팅방에 접속하여 메세지를 수신했다고 다른 참가자들에게 전송합니다.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("readmessage")]
        public async Task<IActionResult> UpdateLastReadedMessage([FromBody] UpdateLastReadedMessageRequest request)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail)) return Unauthorized();

            try
            {
                // 1. 해당 방의 참여지인지 검사
                UserReadUpdateResponse? response = await _chatService.UpdateReadStatusAsync(CurrentUserEmail, request);
                if (response == null) return NotFound("해당 채팅방에 접근 권한이 없습니다.");

                // 2. ChatHub를 통해 해당 방의 모든 접속자에게 브로드캐스트
                // 참여자들에게 내가 메세지를 읽었다고 전달함.
                await _chatHubContext.Clients.Group(request.RoomId.ToString())
                    .SendAsync(ChatHubEvents.ChatHubResponseEvent.UserReadMessage, response);
                return Ok();
            }
            catch
            {
                return BadRequest("메세지 읽음 처리 실패");
            }
        }
        #endregion
    }
}
