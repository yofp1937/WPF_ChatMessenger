using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.Common;

namespace ChatMessenger.Server.Interfaces.Services.Repositories
{
    public interface IChatParticipantRepositoryService : IBaseRepositoryService
    {
        /// <summary>
        /// User의 Email로 특정 채팅방의 ChatParticipant Entity를 반환해줍니다.
        /// </summary>
        /// <remarks>
        /// AsNoTracking이 적용되어 추적 수정 기능이 사용 불가능합니다.
        /// </remarks>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">ChatParticipant Entity 찾을 User의 Email</param>
        /// <returns>Entity 있으면 ChatParticipant Entity, 없으면 null</returns>
        Task<ChatParticipant?> GetParticipantEntityAsync(Guid roomId, string userEmail);
        /// <summary>
        /// User의 Email로 특정 채팅방의 ChatParticipant Entity를 반환해줍니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">ChatParticipant Entity 찾을 User의 Email</param>
        /// <returns>Entity 있으면 ChatParticipant Entity, 없으면 null</returns>
        Task<ChatParticipant?> GetTrackingParticipantEntityAsync(Guid roomId, string userEmail);
        /// <summary>
        /// 특정 채팅방 참가자들의 ChatParticipantProjection List를 추출해 반환해줍니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>특정 채팅방 참가자들의 ChatParticipantProjection List</returns>
        Task<List<ChatParticipantProjection>> GetParticipantProjectionListAsync(Guid roomId);
        /// <summary>
        /// ChatParticipant Entity를 업데이트합니다. (Entity는 추적(Tracking) 상태여야합니다.)
        /// </summary>
        /// <remarks>
        /// 대리자를 사용해 메모리상의 participant 객체의 속성을 수정하면 EntityFramework가 실제 수정된 컬럼만 업데이트합니다.
        /// </remarks>
        /// <param name="participant">업데이트할 ChatParticipant Entity 객체</param>
        /// <param name="updateAction">Entity의 속성을 수정하는 로직 (p => p.LastReadMessageId = 1)</param>
        /// <returns>업데이트 성공시 true, 실패시 false</returns>
        Task<bool> UpdateChatParticipantAsync(ChatParticipant participant, Action<ChatParticipant> updateAction);
        /// <summary>
        /// 특정 채팅방의 총 참가자 수를 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>채팅방의 총 참가자 수</returns>
        Task<int> GetParticipantsCountAsync(Guid roomId);
        /// <summary>
        /// 채팅방 참가자들의 ChatParticipantDTO를 추출하여 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<List<ChatParticipantDTO>> GetParticipantDTOListAsync(Guid roomId);
        /// <summary>
        /// 채팅 참가자 데이터를 삭제합니다.
        /// </summary>
        /// <param name="participant">삭제할 ChatParticipant Entity</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<bool> RemoveParticipantAsync(ChatParticipant participant);
        /// <summary>
        /// 채팅방에 참가자들을 등록합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="emails">추가하려는 참가자들의 Email이 담긴 List</param>
        /// <returns>요청 결과 Data가 담긴 ServiceResult</returns>
        Task<bool> AddParticipantsToRoomAsync(Guid roomId, IEnumerable<string> emails);
    }
}
