/*
 * ChatRoomView에서 표시될 채팅방의 상세한 정보를 담고있는 모델
 */
using ChatMessenger.Client.Models.Friends;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
namespace ChatMessenger.Client.Models.Chats
{
    public partial class ChatRoomDetailModel : ObservableObject
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
        // 실제 참여자 상세 정보 목록
        [ObservableProperty]
        private ObservableCollection<FriendModel> _participants = new();

        // 채팅 내역
        [ObservableProperty]
        private ObservableCollection<ChatMessageModel> _messages = new();

        // 읽지 않은 메세지 수
        [ObservableProperty]
        private int _unreadCount;

        // 마지막으로 내가 읽은 메시지 ID
        // 서버와 동기화하여 unreadCount를 계산할 때 기준이 됨
        public long LastReadMessageId { get; set; }
    }
}
