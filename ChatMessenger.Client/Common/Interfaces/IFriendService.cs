/*
 * 사용자의 친구 목록을 관리해줄 서비스 Interface
 */
using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IFriendService
    {
        /// <summary>
        /// 서버에서 내 친구 목록을 비동기로 가져옵니다.
        /// </summary>
        Task<List<FriendResponse>?> GetFriendsListAsync();
        /// <summary>
        /// 서버에 친구를 검색하여 데이터를 요청합니다.
        /// </summary>
        Task<FriendResponse?> SearchFriendAsync(string email);
    }
}
