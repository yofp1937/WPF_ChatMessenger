namespace ChatMessenger.Shared.DTOs.Requests.Chat
{
    /// <summary>
    /// 그룹 채팅방 생성을 요청하기위한 DTO입니다.
    /// </summary>
    public class CreateGroupChatRequest
    {
        // 방 제목
        public string Title { get; set; } = string.Empty;
        // 채팅방 프로필 이미지
        public string? ProfileImageURL = null;
        // 초대할 대상들의 Email 목록
        public List<string> TargetEmails { get; set; } = new();
    }
}
