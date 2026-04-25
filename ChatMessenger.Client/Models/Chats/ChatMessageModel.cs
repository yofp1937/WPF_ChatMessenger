using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.Models.Chats
{
    public partial class ChatMessageModel : ObservableObject
    {
        public long MessageId { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderNickname { get; set; } = string.Empty;
        public string SenderProfileImage { get; set; } = string.Empty;

        // 내용
        [ObservableProperty]
        private string _content = string.Empty;

        // 전송 시간
        [ObservableProperty]
        private DateTime _sentAt;

        // 내가 보낸 메시지인지 여부 (View에서 왼쪽/오른쪽 정렬을 결정하는 용도)
        public bool IsMe { get; set; }

        // 메시지 안 읽은 사람 수
        [ObservableProperty]
        private int _unreadPeopleCount;
    }
}
