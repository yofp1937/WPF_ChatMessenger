using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Services.Base;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatMessenger.Client.Common.Services.Chats
{
    public partial class ChatService : BaseService, IChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #region public Method
        #region 채팅방 조회
        /// <inheritdoc/>
        /// <remarks>
        /// Server에게 Response를 전달받고, ChatRoomSummaryModel 형식으로 매핑해서 반환합니다.
        /// </remarks>
        public async Task<ServiceResult<ChatRoomSummaryModel>> GetChatRoomSummaryModelAsync(Guid roomId)
        {
            // 1. BaseService의 ExecuteAsync 실행
            return await ExecuteAsync<ChatRoomSummaryResponse, ChatRoomSummaryModel>(
                // 2. Http 통신 요청 정의
                sendRequestFunc: () => _httpClient.GetAsync($"api/chat/getchatroom/{roomId}"),
                // 3. 서버 응답 코드가 200(Ok)으로 넘어오면 DTO 변환 규칙 정의
                mapToModelFunc: (response) => new ChatRoomSummaryModel(response)
                );
        }
        /// <inheritdoc/>
        /// <remarks>
        /// Server에게 Response를 전달받고, Response를 List<ChatRoomSummaryModel> 형식으로 매핑해서 반환합니다.
        /// </remarks>
        public async Task<ServiceResult<List<ChatRoomSummaryModel>>> GetChatRoomSummaryModelListAsync()
        {
            return await ExecuteAsync<List<ChatRoomSummaryResponse>, List<ChatRoomSummaryModel>>(
                sendRequestFunc: () => _httpClient.GetAsync($"api/chat/getchatroomlist"),
                mapToModelFunc: (dtoList) => dtoList.Select(dto => new ChatRoomSummaryModel(dto)).ToList()
                );
        }
        /// <inheritdoc/>
        /// <remarks>
        /// Server에게 Response를 전달받고, ChatRoomDetailModel 형식으로 매핑해서 반환합니다.
        /// </remarks>
        public async Task<ServiceResult<ChatRoomDetailModel>> GetChatRoomDetailModelAsync(Guid roomId, string myEmail)
        {
            return await ExecuteAsync<ChatRoomDetailResponse, ChatRoomDetailModel>(
                sendRequestFunc: () => _httpClient.GetAsync($"api/chat/join/{roomId}"),
                mapToModelFunc: (response) => new ChatRoomDetailModel(response, myEmail)
            );
        }
        #endregion
        #region 채팅방 Add, Remove, Update
        /// <inheritdoc/>
        public async Task<ServiceResult<Guid>> CreateGroupChatAsync(string title, string? profileIMG, List<string> emails)
        {
            // 1. request 객체 생성
            CreateGroupChatRequest request = new()
            {
                Title = title,
                ProfileImageURL = profileIMG,
                TargetEmails = emails,
            };
            return await ExecuteAsync<Guid, Guid>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync("api/chat/creategroupchat", request),
                mapToModelFunc: x => x
                );
        }
        #endregion
        #region 메세지 Add, Remove, Update
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> UpdateLastReadedMessageAsync(UpdateLastReadedMessageRequest request)
        {
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync("api/chat/readmessage", request),
                mapToModelFunc: x => x
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> SendMessageAsync(SendMessageRequest request)
        {
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync("api/chat/sendmessage", request),
                mapToModelFunc: x => x
                );
        }
        #endregion 메세지 Add, Remove, Update
        #region 채팅 참가자 Add, Remove, Update
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> LeaveRoomAsync(Guid roomId)
        {
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync($"api/chat/leave/{roomId}", roomId),
                mapToModelFunc: x => x
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> InviteParticipantsAsync(Guid roomId, List<string> emails)
        {
            // 1. request 생성
            InviteParticipantsRequest request = new()
            {
                RoomId = roomId,
                ParticipantEmails = emails
            };
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync("api/chat/invite", request),
                mapToModelFunc: x => x
                );
        }
        #endregion 채팅 참가자 Add, Remove, Update






        /// <inheritdoc/>
        public async Task<ChatRoomSummaryModel?> CreatePrivateChatRoomAsync(string targetEmail)
        {
            try
            {
                // 1. request 객체 생성
                CreatePrivateChatRequest request = new() { TargetEmail = targetEmail };

                // 2. Server로 개인 채팅방 생성 요청
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/chat/searchorcreateroom", request);
                if (!response.IsSuccessStatusCode) return null;

                // 3. response에서 ChatRoomSummaryResponse 추출
                ChatRoomSummaryResponse? result = await response.Content.ReadFromJsonAsync<ChatRoomSummaryResponse>();
                if (result == null) return null;

                // 4. ChatRoomSummaryModel로 변환하여 반환
                return new ChatRoomSummaryModel(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(CreatePrivateChatRoomAsync)}]: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
