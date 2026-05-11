namespace ChatMessenger.Shared.DTOs.Requests
{
    /// <summary>
    /// 채팅방에 메세지를 전달할때 Server에게 전송하는 DTO 입니다.
    /// </summary>
    public class SendMessageRequest
    {
        public Guid RoomId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
