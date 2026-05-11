using ChatMessenger.Server.Data.Entities;

namespace ChatMessenger.Server.Data.Projections
{
    /// <summary>
    /// 채팅방 목록 조회시 효율적인 데이터 추출을 위해 정의된 Projection입니다.
    /// </summary>
    /// <remarks>
    /// Db Entity의 원본을 가공하여 ChatRoomSummaryResponse를 생성하기 위한 중간 DTO입니다.<br/>
    /// 복잡한 Join 결과를 단일 객체로 캡슐화하여 컨트롤러 로직을 간소화시킵니다.
    /// </remarks>
    public class ChatRoomSummaryDTO
    {
        public Guid ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;
        public bool IsGroupChat { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TargetUserNickname { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? LastMessageSentAt { get; set; }
        public long LastReadMessageId { get; set; }
        public int ParticipantCount { get; set; }
        public int UnreadCount { get; set; }
    }
}
