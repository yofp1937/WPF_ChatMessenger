using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IChatHubService
    {
        // 서버로부터 갱신 메세지가 전달되면 ViewModel에게 전달하기위한 event들
        event Action<ChatMessageResponse> MessageReceivedEvent;
        event Action<UserReadUpdateResponse> ReadStatusUpdatedEvent;

        Task ConnectAsync(string accessToken);
        Task DisconnectAsync();
        Task JoinRoomAsync(Guid roomId);
        Task LeaveRoomAsync(Guid roomId);
    }
}
