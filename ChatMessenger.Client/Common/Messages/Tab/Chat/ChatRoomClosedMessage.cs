namespace ChatMessenger.Client.Common.Messages.Tab.Chat
{
    /// <summary>
    /// ChatRoomViewModel에서 CurrentRoom이 null로 바뀌었을때 ChatListViewModel의 SelectRoom도 null로 바꾸기위해 전송하는 메세지
    /// </summary>
    public record ChatRoomClosedMessage();
}
