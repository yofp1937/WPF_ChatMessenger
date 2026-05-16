namespace ChatMessenger.Shared.DTOs.Requests.Chat
{
    /// <summary>
    /// 1대1 채팅방 생성을 요청하기위한 DTO입니다.
    /// </summary>
    public class CreatePrivateChatRequest
    {
        public string TargetEmail { get; set; } = string.Empty;
    }
}
