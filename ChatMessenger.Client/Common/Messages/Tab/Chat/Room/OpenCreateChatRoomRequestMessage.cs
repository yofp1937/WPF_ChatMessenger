namespace ChatMessenger.Client.Common.Messages.Tab.Chat.Room
{
    /// <summary>
    /// CreateChatRoomView를 표시해야할때 전송하는 메세지입니다.
    /// </summary>
    public class OpenCreateChatRoomRequestMessage
    {
        public Guid? RoomId { get; }
        public string? Title { get; }
        public string? ProfileIMGURL { get; }
        public List<string> ParticipantEmails { get; } = new();

        /// <summary>
        /// 신규 그룹 채팅방을 개설하려할때 View를 띄우기위해 전송하는 메세지의 생성자입니다.
        /// </summary>
        public OpenCreateChatRoomRequestMessage() { }

        /// <summary>
        /// 1대1 채팅방에서 기존 채팅 대상을 포함해 그룹 채팅방을 개설하려할때 맞춤 View를 띄우기위해 전송하는 메세지의 생성자입니다.
        /// </summary>
        /// <param name="participantEmails">기존 채팅 상대의 이메일</param>
        public OpenCreateChatRoomRequestMessage(List<string> participantEmails)
        {
            ParticipantEmails = participantEmails;
        }

        /// <summary>
        /// 그룹 채팅방에서 신규 채팅 대상을 초대하려할때 맞춤 View를 띄우기위해 전송하는 메세지의 생성자입니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        /// <param name="title">채팅방 제목</param>
        /// <param name="profileIMG">프로필 이미지 경로</param>
        /// <param name="participantEmails">이미 참여중인 참가자들의 이메일</param>
        public OpenCreateChatRoomRequestMessage(Guid? roomId, string? title, string? profileIMG, List<string> participantEmails)
        {
            RoomId = roomId;
            Title = title;
            ProfileIMGURL = profileIMG;
            ParticipantEmails = participantEmails;
        }
    }
}
