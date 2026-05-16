using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses.Friend;

namespace ChatMessenger.Server.Interfaces.Friend
{
    /// <summary>
    /// 친구와 관련된 로직(검색, 추가, 상태 변경 등)을 처리하는 Service의 Interface입니다.
    /// </summary>
    public interface IFriendService
    {
        /// <summary>
        /// 친구 목록을 가져옵니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <returns>로그인한 User의 친구 목록</returns>
        Task<List<FriendResponse>> GetFriendListAsync(string myEmail);

        /// <summary>
        /// 유저를 검색합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="friendEmail">검색하려는 User의 Email</param>
        /// <returns>검색한 User의 Profile 정보</returns>
        Task<FriendResponse?> SearchUserAsync(string myEmail, string friendEmail);

        /// <summary>
        /// 유저를 친구로 등록합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="friendEmail">친구 추가 대상 User의 Email</param>
        /// <returns>친구로 추가한 User의 Profile 정보</returns>
        Task<FriendResponse?> AddFriendAsync(string myEmail, string friendEmail);

        /// <summary>
        /// 친구를 친구 목록에서 삭제합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="friendEmail">친구 삭제 대상 User의 Email</param>
        /// <returns>삭제 성공시 true, 삭제 실패시 false 반환</returns>
        Task<bool> DeleteFriendAsync(string myEmail, string friendEmail);

        /// <summary>
        /// 친구의 즐겨찾기 상태를 변경합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">상태 변경 처리에 필요한 데이터가 담긴 Request DTO</param>
        /// <returns>상태 변경 성공시 true, 변경 실패시 false 반환</returns>
        Task<bool> UpdateFavoriteAsync(string myEmail, FriendStatusRequest request);

        /// <summary>
        /// 상대의 차단 상태를 변경합니다.
        /// </summary>
        /// <param name="myEmail">로그인한 User의 Email</param>
        /// <param name="request">상태 변경 처리에 필요한 데이터가 담긴 Request DTO</param>
        /// <returns>상태 변경 성공시 true, 변경 실패시 false 반환</returns>
        Task<bool> UpdateBlockAsync(string myEmail, FriendStatusRequest request);
    }
}
