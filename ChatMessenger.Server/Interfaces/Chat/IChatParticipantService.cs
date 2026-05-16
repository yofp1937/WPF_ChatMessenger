using ChatMessenger.Server.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Server.Common.Interfaces.Chats
{
    public interface IChatParticipantService
    {
        /// <summary>
        /// User가 해당 방의 실제 참가자인지 확인하고 Entity를 반환합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="myEmail">참가여부 확인할 User의 Email</param>
        /// <returns>ChatParticipant Entity</returns>
        Task<ChatParticipant?> GetParticipantEntityAsync(Guid roomId, string myEmail);

        /// <summary>
        /// 채팅방에 참가자들을 등록합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="emails">추가하려는 참가자들의 Email이 담긴 List</param>
        /// <returns>참가자 등록 성공 시 true, 등록 실패 시 false</returns>
        Task<bool> AddParticipantsToRoomAsync(Guid roomId, IEnumerable<string> emails);

        /// <summary>
        /// 채팅방 참가자들의 Email을 추출하여 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>해당 채팅방 참가자들의 Email List</returns>
        Task<List<string>> GetParticipantEmailsAsync(Guid roomId);
        /// <summary>
        /// 채팅방 참가자들의 Nickname을 추출하여 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>해당 채팅방 참가자들의 Nickname List</returns>
        Task<List<string>> GetParticipantNicknamesAsync(Guid roomId);
        /// <summary>
        /// 채팅방의 참가자 수를 가져옵니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>채팅방의 총 참가 인원</returns>
        Task<int> GetParticipantCountAsync(Guid roomId);

        /// <summary>
        /// 채팅 참가자의 데이터를 업데이트하고 DB에 저장합니다.
        /// </summary>
        /// <remarks>
        /// 외부에서 participant의 값을 변경한 후 호출하여 Db에 적용합니다.
        /// </remarks>
        /// <param name="participant">업데이트할 참가자 Entity 객체</param>
        /// <returns>저장 성공 시 true, 실패 시 false</returns>
        Task<bool> UpdateParticipantAsync(ChatParticipant participant);

        /// <summary>
        /// 채팅 참가자 데이터를 삭제합니다.
        /// </summary>
        /// <param name="participant">삭제할 참가자 Entity 객체</param>
        /// <returns>삭제 성공 시 true, 실패 시 false</returns>
        Task<bool> RemoveParticipantAsync(ChatParticipant participant);

        /// <summary>
        /// 해당 채팅방에 참여자가 단 한 명이라도 남아있는지 확인합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <returns>참여자가 존재하면 true, 아무도 없으면 false</returns>
        Task<bool> HasAnyParticipantsAsync(Guid roomId);
    }
}
