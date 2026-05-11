namespace ChatMessenger.Shared.DTOs.Responses
{
    /// <summary>
    /// Server에서 Client측으로 채팅방의 간략한 정보를 전송해주기위한 DTO입니다.
    /// </summary>
    public class ChatRoomSummaryResponse
    {
        // 채팅방 식별 번호
        public Guid RoomId { get; set; }

        // 채팅방 이름
        public string Title { get; set; } = string.Empty;
        // 채팅방 이미지
        public string? RoomProfileImageURL { get; set; }

        // 참가자 수
        public int ParticipantCount { get; set; }

        // 마지막 메세지
        public string LastMessage { get; set; } = string.Empty;
        // 마지막 메세지 전송 시간
        public DateTime? LastMessageSentAt { get; set; }

        // 읽지 않은 메세지 수
        public int UnreadCount { get; set; }

        // 그룹 채팅 여부
        public bool IsGroupChat { get; set; }
    }
}
