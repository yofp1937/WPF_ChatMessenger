namespace ChatMessenger.Client.Common.Messages.Tab.Chat
{
    /// <summary>
    /// ChatRoomViewModel에서 CurretRoom의 모든 메세지를 읽었을때, ChatListViewModel의 UnreadCount도 변경하게해주는 메세지
    /// </summary>
    /// <param name="roomId">방의 식별 번호</param>
    public record ChatRoomReadMarkedMessage(Guid roomId);
}
