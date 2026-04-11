/*
 * 사용자의 친구 목록을 관리해주는 서비스
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Shared.DTOs.Responses;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatMessenger.Client.Common.Services
{
    public class FriendService : IFriendService
    {
        private readonly HttpClient _httpClient;

        public FriendService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<List<FriendResponse>?> GetFriendsListAsync()
        {
            try
            {
                // 1.서버에 친구 목록을 요청함 (토큰은 AuthHeaderHandler에서 자동으로 삽입해줌)
                HttpResponseMessage response = await _httpClient.GetAsync("api/friend/list");
                if (response.IsSuccessStatusCode)
                {
                    // 2.요청이 성공적으로 처리됐으면 반환값을 역직렬화하여 반환
                    return await response.Content.ReadFromJsonAsync<List<FriendResponse>>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FriendService Error]: {ex.Message}");
                return null;
            }
        }
    }
}
