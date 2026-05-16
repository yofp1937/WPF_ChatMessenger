using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Configs;
using ChatMessenger.Shared.Constants;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatMessenger.Client.Common.Services.Chats
{
    public class ChatHubService : IChatHubService
    {
        private HubConnection? _connection;

        public event Action<ChatMessageResponse>? MessageReceivedEvent;
        public event Action<UserReadUpdateResponse>? ReadStatusUpdatedEvent;

        public async Task ConnectAsync(string accessToken)
        {
            if (_connection?.State == HubConnectionState.Connected) return;
            string hubUrl = $"{DependencyInjectionConfig.ServerBaseUrl}/chathub";

            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken)!;
                })
                .WithAutomaticReconnect()
                .Build();

            // [ResponseEvent 구독] 상수 클래스를 활용하여 안전하게 등록
            _connection.On<ChatMessageResponse>(ChatHubEvents.ChatHubResponseEvent.ReceiveMessage, (res) =>
            {
                MessageReceivedEvent?.Invoke(res);
            });

            _connection.On<UserReadUpdateResponse>(ChatHubEvents.ChatHubResponseEvent.UserReadMessage, (res) =>
            {
                ReadStatusUpdatedEvent?.Invoke(res);
            });

            await _connection.StartAsync();
        }

        public async Task JoinRoomAsync(Guid roomId)
        {
            if (_connection?.State != HubConnectionState.Connected) return;

            // [RequestEvent 호출] 서버의 JoinRoom 메서드 실행
            await _connection.InvokeAsync(ChatHubEvents.ChatHubRequestEvent.JoinRoom, roomId);
        }

        public async Task LeaveRoomAsync(Guid roomId)
        {
            if (_connection?.State != HubConnectionState.Connected) return;
            await _connection.InvokeAsync(ChatHubEvents.ChatHubRequestEvent.LeaveRoom, roomId);
        }

        public async Task DisconnectAsync()
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }
        }
    }
}
