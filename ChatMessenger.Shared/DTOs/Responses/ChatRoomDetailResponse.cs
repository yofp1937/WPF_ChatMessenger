namespace ChatMessenger.Shared.DTOs.Responses
{
    /// <summary>
    /// Server에서 Client측으로 채팅방의 상세 정보를 전송해주기위한 DTO입니다.
    /// </summary>
    public class ChatRoomDetailResponse
    {
        public Guid RoomId { get; set; }
        public string Title { get; set; } = string.Empty;
        // 원래 채팅방 이름
        public string? OriginalRoomTitle { get; set; }
        public string? RoomProfileImageURL { get; set; }
        public int ParticipantCount { get; set; }
        public bool IsGroupChat { get; set; }

        // 방 참여자 목록
        public List<FriendResponse> Participants { get; set; } = new();
        // 채팅 내역
        public List<ChatMessageResponse> Messages { get; set; } = new();

        // 읽지 않은 메세지 수
        public int UnreadCount { get; set; }

        // 사용자가 마지막으로 읽은 메세지
        public long LastReadMessageId { get; set; }
    }
}
