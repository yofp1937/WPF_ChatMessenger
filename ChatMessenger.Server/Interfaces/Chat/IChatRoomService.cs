using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Data.Projections;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Server.Common.Interfaces.Chats
{
    public interface IChatRoomService
    {
        /// <summary>
        /// 채팅방 식별 번호로 채팅방 Entity를 찾아서 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>채팅방 Entity</returns>
        Task<ChatRoom?> GetChatRoomAsync(Guid roomId);
        /// <summary>
        /// 참가자의 Email로 채팅방 Entity를 찾아서 반환합니다.
        /// </summary>
        /// <param name="email1">참가자의 이메일</param>
        /// <param name="email2">참가자의 이메일</param>
        /// <returns>채팅방 Entity</returns>
        Task<ChatRoom?> GetPrivateChatRoomAsync(string email1, string email2);

        /// <summary>
        /// 채팅방을 새로 생성합니다.
        /// </summary>
        /// <param name="title">채팅방 제목</param>
        /// <param name="isGroupChat">그룹 채팅 여부</param>
        /// <returns>새로 등록된 채팅방 Entity</returns>
        Task<ChatRoom?> CreateChatRoomAsync(string? title, bool isGroupChat);

        /// <summary>
        /// User가 입장한 채팅방 중 roomId와 일치하는 방의 정보를 추출하여 반환합니다.
        /// </summary>
        /// <param name="userEmail">채팅방 정보를 추출할 User의 Email</param>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>User가 가입한 채팅방의 ChatRoomSummaryDTO</returns>
        Task<ChatRoomSummaryDTO?> GetChatRoomSummaryDTOAsync(string userEmail, Guid roomId);
        /// <summary>
        /// User가 참여한 모든 채팅방 정보를 추출하여 반환합니다.
        /// </summary>
        /// <param name="userEmail">채팅방 정보를 추출할 User의 Email</param>
        /// <returns>User가 가입한 모든 채팅방의 ChatRoomSummaryDTO</returns>
        Task<List<ChatRoomSummaryDTO>> GetChatRoomSummaryDTOListAsync(string userEmail);

        /// <summary>
        /// ChatRoomDetailResponse 생성에 필요한 DTO를 추출하여 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="userEmail">채팅방 정보를 추출할 User의 Email</param>
        /// <returns>User가 가입한 채팅방의 ChatRoomSummaryDTO</returns>
        Task<ChatRoomDetailDTO?> GetChatRoomDetailDTOAsync(Guid roomId, string userEmail);

        /// <summary>
        /// 특정 채팅방을 Db에서 삭제합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>삭제 성공 시 true, 실패 시 false</returns>
        Task<bool> RemoveChatRoomAsync(Guid roomId);

        /// <summary>
        /// 채팅방 정보를 업데이트합니다.
        /// </summary>
        /// <param name="room">업데이트할 ChatRoom Entity</param>
        /// <returns>업데이트 성공 시 true, 실패 시 false</returns>
        Task<bool> UpdateChatRoomAsync(ChatRoom room);
    }
}
