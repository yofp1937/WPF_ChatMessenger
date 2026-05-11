using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.Models.Chats
{
    // UI와 직접 Binding될 모델이므로 ObservableObject 상속
    /// <summary>
    /// ChatListView에서 표시될 채팅방의 간략한 정보를 담고있는 데이터 모델입니다.
    /// </summary>
    /// <remarks>
    /// 실시간 데이터 변경(읽지 않은 메세지 수 등) 통지를 위해 ObservableObject를 상속받습니다.<br/>
    /// Server에서 넘어온 DTO를 기반으로 생성됩니다.
    /// </remarks>
    public partial class ChatRoomSummaryModel : ObservableObject
    {
        // 채팅방 식별 번호
        public Guid RoomId { get; set; }

        // 채팅방 이름
        [ObservableProperty]
        private string _title = string.Empty;
        // 채팅방 이미지
        [ObservableProperty]
        private string? _roomProfileImageURL = string.Empty;

        // 참가자 수
        [ObservableProperty]
        private int _participiantCount;

        // 마지막 메세지
        [ObservableProperty]
        private string _lastMessage = string.Empty;
        // 마지막 메세지 전송 시간
        [ObservableProperty]
        private DateTime _lastMessageSentAt;

        // 읽지 않은 메세지 수
        [ObservableProperty]
        private int _unreadCount;
        // 메세지 정렬할때 사용
        public bool HasUnreadMessages => UnreadCount > 0;

        [ObservableProperty]
        private bool _isGroupChat;
    }
}
