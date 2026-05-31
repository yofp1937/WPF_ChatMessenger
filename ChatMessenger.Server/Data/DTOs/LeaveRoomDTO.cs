using ChatMessenger.Server.Data.Entities;

namespace ChatMessenger.Server.Data.DTOs
{
    /// <summary>
    /// 특정 유저가 채팅방에 가입하거나, 떠났을때 남아있는 참가자들에게 시스템 메세지를 전송하기위해 사용되는 DTO 
    /// </summary>
    public class JoinAndLeaveRoomDTO
    {
        public Guid RoomId { get; set; } = Guid.Empty;
        // 생성된 입퇴장 시스템 메세지
        public ChatMessage? SystemMessage;
        // 채팅방에 남아있는 유저들의 Email 리스트
        public List<string> RemainingUsersEmailList { get; set; } = new();
    }
}
