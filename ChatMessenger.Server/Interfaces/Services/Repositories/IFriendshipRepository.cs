using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Responses.Friend;

namespace ChatMessenger.Server.Interfaces.Services.Repositories
{
    public interface IFriendshipRepository : IBaseRepositoryService
    {
        /// <summary>
        /// userEmail의 친구 목록을 FriendResponse DTO 형태로 최적화하여 반환합니다.
        /// </summary>
        /// <param name="userEmail">친구 목록을 조회할 User의 Email</param>
        /// <returns>친구 목록 리스트</returns>
        Task<List<FriendResponse>> GetFriendResponseListAsync(string userEmail);
        /// <summary>
        /// userEmail과 friendEmail간의 Friendship Entity를 반환합니다.
        /// </summary>
        /// <param name="userEmail">Friendship Entity의 주체가되는 User의 Email</param>
        /// <param name="friendEmail">Friendship Eintity의 대상이되는 User의 Email</param>
        /// <returns>데이터 존재 시 Friendship Entity, 없으면 null</returns>
        Task<Friendship?> GetFriendshipEntityAsync(string userEmail, string friendEmail);
        /// <summary>
        /// userEmail과 friendEmail간의 Friendship Entity를 생성합니다.
        /// </summary>
        /// <param name="userEmail">Friendship Entity의 주체가되는 User의 Email</param>
        /// <param name="friendEmail">Friendship Eintity의 대상이되는 User의 Email</param>
        /// <param name="isFavorite">즐겨찾기 상태 지정</param>
        /// <param name="isBlocked">차단 상태 지정</param>
        /// <returns>생성 성공 시 true, 실패 시 false</returns>
        Task<bool> AddFriendshipEntityAsync(string userEmail, string friendEmail, bool isFavorite = false, bool isBlocked = false);
        /// <summary>
        /// Friendship Entity를 삭제합니다.
        /// </summary>
        /// <param name="friendship">Db에서 삭제할 Friendship Entity</param>
        /// <returns>삭제 성공 시 true, 실패 시 false</returns>
        Task<bool> RemoveFriendshipEntityAsync(Friendship friendship);
        /// <summary>
        /// Friendship Entity를 업데이트합니다. (Entity는 추적(Tracking) 상태여야합니다.)
        /// </summary>
        /// <remarks>
        /// 대리자를 사용해 메모리상의 Friendship 객체의 속성을 수정하면 EntityFramework가 실제 수정된 컬럼만 업데이트합니다.
        /// </remarks>
        /// <param name="friendship">업데이트할 Friendship Entity 객체</param>
        /// <param name="updateAction">Entity의 속성을 수정하는 로직 (f => f.IsBlocked = true)</param>
        /// <returns>업데이트 성공시 true, 실패시 false</returns>
        Task<bool> UpdateFriendshipAsync(Friendship friendship, Action<Friendship> updateAction);
    }
}
