using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatMessenger.Client.Common.Services.Chats
{
    public partial class ChatService : IChatService
    {
        private readonly HttpClient _httpClient;

        public ChatService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                return response.Select(dto => new ChatRoomSummaryModel(dto)).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(GetMyChatRoomsAsync)}]: {ex.Message}");
                return null;
            }
        }

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

        /// <inheritdoc/>
        public async Task<ChatRoomDetailModel?> GetChatRoomDetailAsync(Guid roomId, string myEmail)
        {
            try
            {
                // Server에 채팅방 상세 데이터 요청
                ChatRoomDetailResponse? response = await _httpClient.GetFromJsonAsync<ChatRoomDetailResponse>($"api/chat/{roomId}");
                if (response == null) return null;

                // Server로부터 전달받은 DTO를 DataModel로 변경
                ChatRoomDetailModel result = new(response, myEmail);

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(GetChatRoomDetailAsync)}]: {ex.Message}");
                return null;
            }
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
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(UpdateLastReadedMessageAsync)}]: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SendMessageAsync(SendMessageRequest request)
        {
            try
            {
                // request 요청
                HttpResponseMessage httpMessage = await _httpClient.PostAsJsonAsync("api/chat/sendmessage", request);
                if(httpMessage == null) return false;

                return httpMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(SendMessageAsync)}]: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> LeaveRoomAsync(Guid roomId)
        {
            try
            {
                HttpResponseMessage httpMessage = await _httpClient.PostAsJsonAsync($"api/chat/leave/{roomId}", roomId);
                if (httpMessage == null) return false;

                return httpMessage.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(LeaveRoomAsync)}]: {ex.Message}");
                return false;
            }
        }

        public async Task<ChatRoomSummaryModel?> CreateGroupChatAsync(string title, string? profileIMG, List<string> emails)
        {
            try
            {
                // 1. request 객체 생성
                CreateGroupChatRequest request = new()
                {
                    Title = title,
                    ProfileImageURL = profileIMG,
                    TargetEmails = emails,
                };

                // 2. Server로 개인 채팅방 생성 요청
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/chat/creategroupchat", request);
                if (!response.IsSuccessStatusCode) return null;

                // 3. response에서 ChatRoomSummaryResponse 추출
                ChatRoomSummaryResponse? result = await response.Content.ReadFromJsonAsync<ChatRoomSummaryResponse>();
                if (result == null) return null;

                // 4. ChatRoomSummaryModel로 변환하여 반환
                return new ChatRoomSummaryModel(result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(ChatService)}_{nameof(CreateGroupChatAsync)}]: {ex.Message}");
                return null;
            }
        }
        #endregion
    }
}
