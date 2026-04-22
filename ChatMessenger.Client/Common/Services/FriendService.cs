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
                Debug.WriteLine($"[{GetType().Name}_GetFriendsListAsync] - Error: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<FriendResponse?> SearchFriendAsync(string email)
        {
            try
            {
                // email을 담아서 검색 요청
                HttpResponseMessage response = await _httpClient.GetAsync($"api/friend/searchuser?email={email}");
                if(response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<FriendResponse>();
                }
                // 실패시 서버에서 전송한 텍스트 메세지를 담아서 Exception 던짐
                string errorMsg = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMsg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{GetType().Name}_SearchFriendAsync] - Error: {ex.Message}");
                throw;
            }
        }
    }
}
