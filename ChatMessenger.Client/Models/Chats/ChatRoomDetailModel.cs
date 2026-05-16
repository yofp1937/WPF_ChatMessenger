/*
 * ChatRoomView에서 표시될 채팅방의 상세한 정보를 담고있는 모델
 */
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Shared.DTOs.Responses.Chat;
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
        public ICollectionView SortedMessages { get; }

        // 읽지 않은 메세지 수
        [ObservableProperty]
        private int _unreadCount;

        // 마지막으로 내가 읽은 메시지 ID
        // 서버와 동기화하여 unreadCount를 계산할 때 기준이 됨
        public long LastReadMessageId { get; set; }

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
    }
}
