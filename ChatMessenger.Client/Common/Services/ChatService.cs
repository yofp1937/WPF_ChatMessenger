using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatMessenger.Client.Common.Services
{
    public partial class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;
        // 로그인한 유저의 이메일 확인용
        private readonly IIdentityService _identityService;

        public ChatService(HttpClient httpClient, IIdentityService identityService)
        {
            _httpClient = httpClient;
            _identityService = identityService;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<List<ChatRoomSummaryModel>?> GetMyChatRoomsAsync()
        {
            try
            {
                // 1. 서버 API 호출 (채팅 목록 요청)
                List<ChatRoomSummaryResponse>? response = await _httpClient.GetFromJsonAsync<List<ChatRoomSummaryResponse>>("api/chat/getchatroomlist");
                if (response == null) return null;

                // 2. 반환받은 DTO를 View에서 사용할 Model로 변환하여 반환
                return response.Select(dto => new ChatRoomSummaryModel
                {
                    RoomId = dto.RoomId,
                    Title = dto.Title ?? string.Empty,
                    RoomProfileImageURL = dto.RoomProfileImageURL ?? string.Empty,
                    ParticipiantCount = dto.ParticipantCount,
                    LastMessage = dto.LastMessage ?? string.Empty,
                    LastMessageSentAt = dto.LastMessageSentAt?.ToLocalTime() ?? DateTime.MinValue,
                    UnreadCount = dto.UnreadCount,
                    IsGroupChat = dto.IsGroupChat,
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{this.GetType().Name}_GetMyChatRoomsAsync - Error]: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<Guid?> CreatePrivateChatRoomAsync(string targetEmail)
        {
            try
            {
                CreatePrivateChatRequest request = new() { TargetEmail = targetEmail };

                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/chat/searchorcreateroom", request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Guid>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{this.GetType().Name}_CreatePrivateChatRoomAsync - Error]: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<ChatRoomDetailModel?> GetChatRoomDetailAsync(Guid roomId)
        {
            // 내 이메일 확인
            if (_identityService.MyProfile == null) return null;
            string myEmail = _identityService.MyProfile.Email;

            // Server에 채팅방 상세 데이터 요청
            ChatRoomDetailResponse? response = await _httpClient.GetFromJsonAsync<ChatRoomDetailResponse>($"api/chat/{roomId}");
            if (response == null) return null;

            // Server로부터 전달받은 DTO를 DataModel로 변경
            ChatRoomDetailModel result = new(response, myEmail);

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateLastReadedMessageAsync(UpdateLastReadedMessageRequest request)
        {
            try
            {
                HttpResponseMessage? response = await _httpClient.PostAsJsonAsync("api/chat/readmessage", request);
                if (response == null) return false;

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateLastReadedMessageAsync Error]: {ex.Message}");
                return false;
            }
        }

        public async Task<bool?> SendMessageAsync(SendMessageRequest request)
        {
            try
            {
                // request 요청
                HttpResponseMessage httpMessage = await _httpClient.PostAsJsonAsync("api/chat/sendmessage", request);
                if (httpMessage.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{GetType().ToString()} - SendMessageAsync]: {ex.Message}");
                return false;
            }
        }
        #endregion
    }
}
