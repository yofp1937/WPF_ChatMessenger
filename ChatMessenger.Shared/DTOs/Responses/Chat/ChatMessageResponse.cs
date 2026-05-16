using ChatMessenger.Shared.DTOs.Responses.Friend;
using ChatMessenger.Shared.Enums;

namespace ChatMessenger.Shared.DTOs.Responses.Chat
{
    /// <summary>
    /// Server에서 Client 측으로 특정 채팅 메세지 정보를 전송해주기위한 DTO입니다.
    /// </summary>
    public class ChatMessageResponse
    {
        public Guid RoomId { get; set; }
        public long MessageId { get; set; }
        public ChatMessageType MessageType { get; set; }
        public FriendResponse? Sender { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public int UnreadPeopleCount { get; set; }
    }
}
