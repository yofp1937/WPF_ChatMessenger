namespace ChatMessenger.Client.Common.Messages.Tab.Chat.Room
{
    /// <summary>
    /// ChatListView에서 사용자가 채팅방을 선택하면 메세지에 채팅방 식별 번호를 담아 화면을 표시해달라고 요청합니다.
    /// </summary>
    /// <remarks>
    /// ContentPanelViewModel에서 해당 메세지를 구독하여 메세지를 받으면 ChatRoomViewModel을 세팅합니다.
    /// </remarks>
    public record ChatRoomSelectionChangedMessage(Guid roomId);
}
