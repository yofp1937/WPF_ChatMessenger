using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Shared.DTOs.Responses;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.Models.Chats
{
    public partial class ChatMessageModel : ObservableObject
    {
        public long MessageId { get; set; }
        public FriendModel? Sender { get; set; }
        // 내용
        [ObservableProperty]
        private string _content = string.Empty;
        // 전송 시간
        [ObservableProperty]
        private DateTime _sentAt;

        // 메시지 안 읽은 사람 수
        [ObservableProperty]
        private int _unreadPeopleCount;
        // View에서 '여기까지 읽었습니다.'를 표시해주기위한 bool 변수
        [ObservableProperty]
        private bool _isFirstUnread;

        // 내가 보낸 메시지인지 여부 (View에서 왼쪽/오른쪽 정렬을 결정하는 용도)
        public bool IsMine { get; set; }

        public ChatMessageModel() { }

        /// <summary>
        /// 오버로딩을 활용해 객체를 생성할때 DTO를 넣어주면 자동으로 매핑해줍니다.
        /// </summary>
        /// <param name="dto">서버 응답 DTO</param>
        /// <param name="myEmail">Message의 IsMine 값 결정을 위한 로그인한 유저의 이메일</param>
        public ChatMessageModel(ChatMessageResponse dto, string myEmail)
        {
            if (dto == null || myEmail == null) return;
            UpdateFromDTO(dto, myEmail);
        }

        /// <summary>
        /// 서버에게 받은 DTO 데이터를 바탕으로 모델의 상태를 업데이트합니다
        /// </summary>
        /// <remarks>
        /// ※ 외부에서 해당 메서드로 데이터를 업데이트할땐 반드시 MessageId가 일치하는지 확인하고 업데이트해야합니다.
        /// </remarks>
        /// <param name="dto">서버 응답 DTO</param>
        /// <param name="myEmail">Message의 IsMine 값 결정을 위한 로그인한 유저의 이메일</param>
        public void UpdateFromDTO(ChatMessageResponse dto, string myEmail)
        {
            MessageId = dto.MessageId;
            Content = dto.Content;
            SentAt = dto.SentAt.ToLocalTime();
            UnreadPeopleCount = dto.UnreadPeopleCount;

            if (dto.Sender != null)
            {
                Sender = new(dto.Sender);
                IsMine = dto.Sender.Email == myEmail;
            }
        }
    }
}
