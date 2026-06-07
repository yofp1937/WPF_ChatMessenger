using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Requests.Chat;

namespace ChatMessenger.Server.Interfaces.Services.Repositories
{
    /// <summary>
    /// ChatMessage Entity에 대한 데이터 처리를 담당하는 Repository Interface입니다.<br/>
    /// 비즈니스 로직을 제외한 순수 데이터 처리 기능을 제공합니다.
    /// </summary>
    public interface IChatMessageRepositoryService : IBaseRepositoryService
    {
        /// <summary>
        /// 특정 채팅방의 최근 50개 메세지 목록을 조회합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="entryMessageId">접근 가능한 메세지 식별 번호</param>
        /// <returns>최근 50개 ChatMessage Entity List</returns>
        Task<List<ChatMessage>> GetLastFiftyMessageListAsync(Guid roomId, long entryMessageId);
        /// <summary>
        /// Db에 새로운 메세지를 등록합니다.
        /// </summary>
        /// <param name="userEmail">메세지 작성자의 Email (SystemMessage일 경우 null 허용)</param>
        /// <param name="request">메세지 정보가 담긴 Request DTO</param>
        /// <returns>저장 성공시 등록된 ChatMessage Entity, 실패시 null 반환</returns>
        Task<ChatMessage?> AddMessageAsnyc(string? userEmail, SendMessageRequest request);
        /// <summary>
        /// 특정 채팅방의 최신 메세지 식별 번호를 조회합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>최신 메세지 식별 번호</returns>
        Task<long> GetLastMessageIdAsync(Guid roomId);
    }
}
