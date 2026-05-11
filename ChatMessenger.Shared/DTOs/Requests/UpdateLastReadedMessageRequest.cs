namespace ChatMessenger.Shared.DTOs.Requests
{
    /// <summary>
    /// 마지막으로 읽은 메세지가 변경됐을때 Server에 데이터 갱신을 요청하는 DTO
    /// </summary>
    public class UpdateLastReadedMessageRequest
    {
        public Guid RoomId { get; set; }
        public long LastReadMessageId { get; set; }
    }
}
