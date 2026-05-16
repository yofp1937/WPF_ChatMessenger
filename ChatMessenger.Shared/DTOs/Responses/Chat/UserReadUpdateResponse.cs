namespace ChatMessenger.Shared.DTOs.Responses.Chat
{
    public class UserReadUpdateResponse
    {
        public Guid RoomId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public long LastReadMessageId { get; set; } // 유저가 마지막으로 읽은 메세지
        public long PreviousLastReadMessageId { get; set; } // 유저가 이전에 마지막으로 읽었던 메세지
    }
}
