namespace ChatMessenger.Client.Common.Messages.Tab.Chat
{
    /// <summary>
    /// ChatListView에서 사용자가 채팅방을 선택하면 ChatRoomDetailView에 RoomId를 전달하는 메세지입니다.
    /// </summary>
    public record ChatRoomSelectionChangedMessage(Guid roomId);
}
