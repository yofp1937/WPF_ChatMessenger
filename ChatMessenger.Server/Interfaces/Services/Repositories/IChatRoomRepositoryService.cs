using ChatMessenger.Server.Data.DTOs;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Responses.Chat;

namespace ChatMessenger.Server.Interfaces.Services.Repositories
{
    public interface IChatRoomRepositoryService : IBaseRepositoryService
    {

        /// <summary>
        /// ChatParticipant Entity를 사용해 ChatRoomSummaryDTO를 추출하여 반환합니다.
        /// </summary>
        /// <param name="participant">채팅방 정보를 추출할 User의 ChatParticipant Entity</param>
        /// <returns>채팅방의 ChatRoomSummaryDTO, 없으면 null</returns>
        Task<ChatRoomSummaryDTO?> GetChatRoomSummaryDTOAsync(ChatParticipant participant);
        /// <summary>
        /// User가 참여한 모든 채팅방의 ChatRoomSummaryDTO를 추출하여 반환합니다.
        /// </summary>
        /// <param name="userEmail">채팅방 정보를 추출할 User의 Email</param>
        /// <returns>채팅방들의 ChatRoomSummaryDTO List</returns>
        Task<List<ChatRoomSummaryDTO>> GetChatRoomSummaryDTOListAsync(string userEmail);
        /// <summary>
        /// 특정 채팅방을 Db에서 삭제합니다.
        /// </summary>
        /// <param name="room">채팅방 ChatRoom Entity</param>
        /// <returns>삭제 성공했을시 true, 실패시 false</returns>
        Task<bool> RemoveRoomAsync(ChatRoom room);
        /// <summary>
        /// 채팅방을 새로 생성합니다.
        /// </summary>
        /// <param name="title">채팅방 제목</param>
        /// <param name="profileImageURL">채팅방 프로필 이미지</param>
        /// <param name="isGroupChat">그룹 채팅 여부</param>
        /// <returns>생성된 ChatRoom Entity, 생성 실패시 null</returns>
        Task<ChatRoom?> CreateChatRoomAsync(string? title, string? profileImageURL, bool isGroupChat);
        /// <summary>
        /// 채팅방 정보를 업데이트합니다.
        /// </summary>
        /// <param name="room">업데이트할 ChatRoom Entity</param>
        /// <param name="updateAction">Entity의 속성을 수정하는 로직 (r => r.RoomProfileImageURL = string.Empty)</param>
        /// <returns>업데이트 성공시 true, 실패시 false</returns>
        Task<bool> UpdateChatRoomAsync(ChatRoom room, Action<ChatRoom> updateAction);
    }
}
