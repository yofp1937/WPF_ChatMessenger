using ChatMessenger.Shared.DTOs.Responses.Chat;
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
        private string? _lastMessage = string.Empty;
        // 마지막 메세지 전송 시간
        [ObservableProperty]
        private DateTime? _lastMessageSentAt;

        // 읽지 않은 메세지 수
        [ObservableProperty]
        private int _unreadCount;
        // 메세지 정렬할때 사용
        public bool HasUnreadMessages => UnreadCount > 0;

        [ObservableProperty]
        private bool _isGroupChat;

        public ChatRoomSummaryModel() { }

        public ChatRoomSummaryModel(ChatRoomSummaryResponse dto)
        {
            UpdateFromDTO(dto);
        }

        /// <summary>
        /// 해당 방의 UnreadCount를 0으로 변경합니다.
        /// </summary>
        public void ClearUnreadCount()
        {
            UnreadCount = 0;
        }

        /// <summary>
        /// 마지막으로 전송받은 메세지를 업데이트하고, UnreadCount를 증가시킬지 정합니다.
        /// </summary>
        /// <param name="message">마지막으로 전송받은 메세지 내용</param>
        /// <param name="sentAt">메세지의 전송 시간</param>
        /// <param name="shouldIncrementUnread">읽지 않음 수를 올릴지 말지 여부 true: 올림, false: 안올림</param>
        public void UpdateLastMessage(string message, DateTime sentAt, bool shouldIncrementUnread)
        {
            LastMessage = message;
            LastMessageSentAt = sentAt.ToLocalTime();
            if (shouldIncrementUnread)
                UnreadCount++;
        }

        /// <summary>
        /// 입력받은 DTO를 바탕으로 모델 데이터를 갱신합니다.
        /// </summary>
        private void UpdateFromDTO(ChatRoomSummaryResponse dto)
        {
            RoomId = dto.RoomId;
            Title = dto.Title;
            RoomProfileImageURL = dto.RoomProfileImageURL;
            ParticipiantCount = dto.ParticipantCount;
            LastMessage = dto.LastMessage;
            LastMessageSentAt = dto.LastMessageSentAt?.ToLocalTime();
            UnreadCount = dto.UnreadCount;
            IsGroupChat = dto.IsGroupChat;
        }
    }
}
