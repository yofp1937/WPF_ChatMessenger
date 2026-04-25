/*
 * ChatListView에서 표시될 채팅방의 간략한 정보를 담고있는 모델
 */
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.Models.Chats
{
    // UI와 직접 Binding될 모델이므로 ObservableObject 상속
    public partial class ChatRoomSummaryModel : ObservableObject
    {
        // 채팅방 식별 번호
        public int RoomId { get; set; }

        // 채팅방 이름
        [ObservableProperty]
        private string _title = string.Empty;
        // 채팅방 이미지
        [ObservableProperty]
        private string _roomProfileImageURL = string.Empty;

        // 참가자 수
        [ObservableProperty]
        private int _participiantCount;

        // 마지막 메세지
        [ObservableProperty]
        private string _lastMessage = string.Empty;
        // 마지막 메세지 전송 시간
        [ObservableProperty]
        private string _lastMessageTime = string.Empty;

        // 읽지 않은 메세지 수
        [ObservableProperty]
        private int _unreadCount;
    }
}
