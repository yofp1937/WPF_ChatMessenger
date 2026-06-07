namespace ChatMessenger.Server.Data.DTOs
{
    /// <summary>
    /// 채팅방 참가자들의 Email, Nickname, IsLeft 같은 단순한 정보들만 추출하기위한 DTO 클래스입니다.
    /// </summary>
    public class ChatParticipantDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
    }
}
