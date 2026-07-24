/*
 * ChatRoomView에서 표시될 채팅방의 상세한 정보를 담고있는 모델
 */
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
namespace ChatMessenger.Client.Models.Chats
{
    public partial class ChatRoomDetailModel : ObservableObject
    {
        // 채팅방 식별 번호
        public Guid RoomId { get; set; }

        // 채팅방 이름
        [ObservableProperty]
        private string _title = string.Empty;
        // 채팅방 이미지
        [ObservableProperty]
        private string? _roomProfileImageURL;
        public bool IsGroupChat { get; set; }

        // 참가자 수
        [ObservableProperty]
        private int _participantCount;
        // 실제 참여자 상세 정보 목록
        [ObservableProperty]
        private ObservableCollection<FriendModel> _participants = new();

        // 채팅 내역
        private ObservableCollection<ChatMessageModel> _messages = new();
        public IReadOnlyList<ChatMessageModel> Messages => _messages;
        public ICollectionView SortedMessages { get; }

        // 읽지 않은 메세지 수
        [ObservableProperty]
        private int _unreadCount;

        // 마지막으로 내가 읽은 메시지 ID
        // 서버와 동기화하여 unreadCount를 계산할 때 기준이 됨
        public long LastReadMessageId { get; set; }

        // 참가자별 마지막으로 읽은 메세지 위치 (Email -> LastReadMessageId)
        // 메세지의 UnreadPeopleCount는 이 위치 집합에서 매번 파생 계산한다.
        // 카운트를 직접 증감(--)하지 않으므로, 중복 이벤트나 재입장에도 값이 어긋나지 않는다.
        private readonly Dictionary<string, long> _readPositions = new();

        public ChatRoomDetailModel()
        {
            SortedMessages = CollectionViewSource.GetDefaultView(_messages);
            // 1순위: MessageId 기준 오름차순
            SortedMessages.SortDescriptions.Add(new SortDescription("MessageId", ListSortDirection.Ascending));
        }

        /// <summary>
        /// 오버로딩을 활용해 객체를 생성할때 DTO를 넣어주면 자동으로 매핑해줍니다.
        /// </summary>
        /// <remarks>
        /// ChatMessageModel의 IsMe를 결정하기위해 myEmail을 받아야합니다.
        /// </remarks>
        /// <param name="dto">서버 응답 DTO</param>
        /// <param name="myEmail">로그인한 유저의 Email</param>
        public ChatRoomDetailModel(ChatRoomDetailResponse dto, string myEmail) : this()
        {
            if (dto == null) return;
            UpdateFromDTO(dto, myEmail);
        }

        /// <summary>
        /// 서버에게 받은 DTO 데이터를 바탕으로 모델의 상태를 업데이트합니다
        /// </summary>
        /// <remarks>
        /// ※ 외부에서 해당 메서드로 데이터를 업데이트할땐 반드시 MessageId가 일치하는지 확인하고 업데이트해야합니다.
        /// </remarks>
        /// <param name="dto">서버 응답 DTO</param>
        /// <param name="myEmail">로그인한 유저의 Email</param>
        public void UpdateFromDTO(ChatRoomDetailResponse dto, string myEmail)
        {
            this.RoomId = dto.RoomId;
            this.Title = dto.Title;
            this.RoomProfileImageURL = dto.RoomProfileImageURL;
            this.ParticipantCount = dto.ParticipantCount;
            this.IsGroupChat = dto.IsGroupChat;

            this.Participants = new(dto.Participants.Select(p => new FriendModel(p)));

            // 참가자별 읽은 위치 맵을 서버 값으로 초기화 (이후 UnreadPeopleCount 파생의 기준)
            _readPositions.Clear();
            foreach (KeyValuePair<string, long> position in dto.ParticipantReadPositions)
                _readPositions[position.Key] = position.Value;

            _messages.Clear();
            bool isFirstUnreadFound = false;
            IEnumerable<ChatMessageModel> tempMessages = dto.Messages.Select(m =>
            {
                ChatMessageModel message = new(m, myEmail);

                // 2. Message의 IsFirstUnread('여기까지 읽었습니다.' 표시용 변수) 설정
                if (!isFirstUnreadFound && m.MessageId > dto.LastReadMessageId)
                {
                    message.IsFirstUnread = true;
                    isFirstUnreadFound = true;
                }
                return message;
            });
            foreach (ChatMessageModel msg in tempMessages)
                _messages.Add(msg);

            this.UnreadCount = dto.UnreadCount;
            this.LastReadMessageId = dto.LastReadMessageId;

            // 초기 메세지들의 UnreadPeopleCount도 위치 맵에서 파생 계산해 서버 기준과 정합성을 맞춘다.
            RecalculateUnreadCounts();
        }

        /// <summary>
        /// 외부에서 SortedMessages에 데이터를 추가하고싶을때 호출하는 메서드입니다.
        /// </summary>
        /// <param name="msg">추가하려는 메세지 데이터 모델</param>
        public void AddMessage(ChatMessageModel msg)
        {
            _messages.Add(msg);
            SortedMessages.Refresh();
        }

        /// <summary>
        /// 현재 방의 마지막으로 읽은 메세지 번호와 안읽은 메세지 개수를 갱신합니다.
        /// </summary>
        /// <param name="lastMessageId">마지막으로 읽은 메세지 식별 번호</param>
        public void MarkAsRead(long lastMessageId)
        {
            // 1. 현재 채팅방의 마지막으로 읽은 메세지 번호 갱신
            LastReadMessageId = lastMessageId;
            // 2. UnreadCount를 lastMessageId보다 큰 Message의 개수 값으로 변경 (보통의 상황에서 lastMessageId보다 Id가 큰 메세지는 존재하지않음)
            UnreadCount = _messages.Count(msg => msg.MessageId > LastReadMessageId);
        }

        /// <summary>
        /// 참가자 한 명의 마지막으로 읽은 메세지 위치를 갱신합니다.
        /// </summary>
        /// <remarks>
        /// 읽은 위치는 단조 증가한다. 재입장이나 순서가 뒤바뀐 읽음 이벤트로 더 낮은 값이 들어오면
        /// 무시하여 위치가 역행하는 것을 막는다. 이 멱등성 덕분에 같은 이벤트를 여러 번 처리해도 결과가 안전하다.
        /// </remarks>
        /// <param name="email">갱신할 참가자의 Email</param>
        /// <param name="lastReadMessageId">참가자가 마지막으로 읽은 메세지 식별 번호</param>
        public void UpdateReadPosition(string email, long lastReadMessageId)
        {
            if (string.IsNullOrEmpty(email)) return;
            if (_readPositions.TryGetValue(email, out long existing) && existing >= lastReadMessageId)
                return;
            _readPositions[email] = lastReadMessageId;
        }

        /// <summary>
        /// 채팅방을 나간 참가자를 읽은 위치 집합에서 제거합니다.
        /// </summary>
        /// <param name="email">제거할 참가자의 Email</param>
        public void RemoveReadPosition(string email)
        {
            if (string.IsNullOrEmpty(email)) return;
            _readPositions.Remove(email);
        }

        /// <summary>
        /// 참가자별 읽은 위치 집합을 기준으로 모든 메세지의 안 읽은 사람 수를 다시 계산합니다.
        /// </summary>
        /// <remarks>
        /// 각 메세지의 UnreadPeopleCount = 그 메세지보다 읽은 위치가 뒤처진 참가자 수.<br/>
        /// 증분이 아닌 파생 계산이므로, 읽음/입장/퇴장 이벤트가 몇 번 재처리되든 항상 정확한 값이 나온다.<br/>
        /// 시스템 메세지(입퇴장 알림)는 안 읽은 사람 수를 표시하지 않으므로 계산 대상에서 제외한다.
        /// </remarks>
        public void RecalculateUnreadCounts()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                foreach (ChatMessageModel message in _messages)
                {
                    if (message.MessageType == ChatMessageType.System) continue;
                    message.UnreadPeopleCount = _readPositions.Count(position => position.Value < message.MessageId);
                }
            });
        }
    }
}
