/*
 * 사용자의 친구 목록을 관리해주는 서비스
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Services.Base;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Requests.Friend;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatMessenger.Client.Common.Services
{
    public class FriendService : BaseService, IFriendService
    {
        private readonly HttpClient _httpClient;

        public FriendService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<ServiceResult<List<FriendModel>>> GetFriendsListAsync()
        {
            return await ExecuteAsync<List<FriendResponse>, List<FriendModel>>(
                sendRequestFunc: () => _httpClient.GetAsync("api/friend/getlist"),
                mapToModelFunc: (response) => response.Select(r => new FriendModel(r)).ToList()
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<FriendModel>> AddFriendAsync(string friendEmail)
        {
            // 1. request 생성
            AddorDeleteFriendRequest request = new() { Email = friendEmail };
            return await ExecuteAsync<FriendResponse, FriendModel>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync("api/friend/addfriend", request),
                mapToModelFunc: (response) => new FriendModel(response)
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> DeleteFriendAsync(string friendEmail)
        {
            // 1.request 생성
            AddorDeleteFriendRequest request = new() { Email = friendEmail };
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PostAsJsonAsync("api/friend/deletefriend", request),
                mapToModelFunc: x => x
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> UpdateFavoriteAsync(string friendEmail, bool isFavorite)
        {
            // 1. request 생성
            FriendStatusRequest request = new() { Email = friendEmail, IsFavorite = isFavorite };
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PatchAsJsonAsync("api/friend/changefavorite", request),
                mapToModelFunc: x => x
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<bool>> UpdateBlockAsync(string friendEmail, bool isBlocked)
        {
            // 1.request 생성
            FriendStatusRequest request = new() { Email = friendEmail, IsBlocked = isBlocked };
            return await ExecuteAsync<bool, bool>(
                sendRequestFunc: () => _httpClient.PatchAsJsonAsync("api/friend/changeblock", request),
                mapToModelFunc: x => x
                );
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<FriendModel>> SearchFriendAsync(string friendEmail)
        {
            return await ExecuteAsync<FriendResponse, FriendModel>(
                sendRequestFunc: () => _httpClient.GetAsync($"api/friend/searchuser?friendEmail={friendEmail}"),
                mapToModelFunc: (response) => new FriendModel(response)
                );
        }
    }
}
