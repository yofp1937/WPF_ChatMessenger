/*
 * 사용자의 친구 목록을 관리해줄 서비스 Interface
 */
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Shared.Common;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IFriendService
    {
        /// <summary>
        /// 서버에서 내 친구 목록을 비동기로 가져옵니다.
        /// </summary>
        Task<ServiceResult<List<FriendModel>>> GetFriendsListAsync();
        /// <summary>
        /// 서버에 친구 추가를 요청합니다.
        /// </summary>
        /// <param name="friendEmail">추가하려는 친구의 이메일</param>
        Task<ServiceResult<FriendModel>> AddFriendAsync(string friendEmail);
        /// <summary>
        /// 서버에 친구 삭제를 요청합니다.
        /// </summary>
        /// <param name="friendEmail">삭제하려는 친구의 이메일</param>
        Task<ServiceResult<bool>> DeleteFriendAsync(string friendEmail);
        /// <summary>
        /// 친구의 즐겨찾기 상태를 변경합니다.
        /// </summary>
        /// <param name="friendEmail">값 변경하려는 친구의 이메일</param>
        /// <param name="isFavorite">변경하려는 즐겨찾기 상태</param>
        Task<ServiceResult<bool>> UpdateFavoriteAsync(string friendEmail, bool isFavorite);
        /// <summary>
        /// 친구의 차단 상태를 변경합니다.
        /// </summary>
        /// <param name="friendEmail">값 변경하려는 친구의 이메일</param>
        /// <param name="isBlocked">변경하려는 차단 상태</param>
        Task<ServiceResult<bool>> UpdateBlockAsync(string friendEmail, bool isBlocked);
        /// <summary>
        /// 서버에 친구를 검색하여 데이터를 요청합니다.
        /// </summary>
        /// <param name="friendEmail">검색하려는 친구의 이메일</param>
        Task<ServiceResult<FriendModel>> SearchFriendAsync(string friendEmail);
    }
}
