using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages.Tab.Chat;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace ChatMessenger.Client.ViewModels.Tabs.Chats
{
    public partial class ChatRoomViewModel : BaseViewModel
    {
        private readonly IIdentityService _identityService;
        private readonly IChatService _chatService;
        private readonly IChatHubService _chatHubService;

        // 현재 화면에 표시할 채팅방
        [ObservableProperty]
        private ChatRoomDetailModel? _currentRoom;

        // 사이드 패널 표시 상태
        [ObservableProperty]
        private bool _isSidePanelVisible;

        [ObservableProperty]
        private string _inputMessage = string.Empty;

        public ChatRoomViewModel(IIdentityService identityService, IChatService chatService, IChatHubService chatHubService)
        {
            _identityService = identityService;
            _chatService = chatService;
            _chatHubService = chatHubService;

            SubscribeEvents();
        }

        /// <summary>
        /// 화면을 roomId 채팅방 화면으로 변경하고 입장합니다.
        /// </summary>
        /// <param name="roomId">채팅방 식별 번호</param>
        public async void SetChatRoom(Guid roomId)
        {
            await LoadRoomDetailAsync(roomId);
        }

        /// <summary>
        /// ChatHub 이벤트 연결을 해제합니다.
        /// </summary>
        /// <remarks>
        /// ChatHub 이벤트 구독도 해제합니다.
        /// </remarks>
        public override void CleanUp()
        {
            base.CleanUp();
            _chatHubService.MessageReceivedEvent -= OnMessageReceived;
            _chatHubService.ReadStatusUpdatedEvent -= OnReadStatusUpdated;
            CurrentRoom = null;
        }

        #region RelayCommand
        [RelayCommand]
        private void ToggleSidePanel()
        {
            IsSidePanelVisible = !IsSidePanelVisible;
        }
        [RelayCommand]
        private void CloseCurrentRoom()
        {
            CurrentRoom = null;
            IsSidePanelVisible = false;
            // ChatListViewModel의 SelectRoom과 CurrentRoom을 동기화하기위해 메세지 전송
            WeakReferenceMessenger.Default.Send(new ChatRoomClosedMessage());
        }
        /// <summary>
        /// 메세지 전송 버튼과 연결된 RelayCommand
        /// </summary>
        /// <remarks>
        /// InputMessage TextBox에 입력된 내용을 채팅방에 전송합니다.
        /// </remarks>
        [RelayCommand]
        private async Task SendMessage()
        {
            if (CurrentRoom == null || string.IsNullOrWhiteSpace(InputMessage)) return;

            // 메세지 전송용 request 생성
            SendMessageRequest request = new()
            {
                RoomId = CurrentRoom.RoomId,
                Content = InputMessage
            };
            // TextBox 비우기
            InputMessage = string.Empty;
            // Server에 메세지 전송 요청
            // 서버에 메세지가 성공적으로 전송되면 OnMessageReceived를 통해 내가 전송한 메세지가 도착
            _ = await _chatService.SendMessageAsync(request);
        }
        [RelayCommand]
        private async Task LeaveRoom()
        {
            if (CurrentRoom == null) return;

            // 1. TODO: 진짜 채팅방 나갈것인지 확인 입력 받아야함

            // 2. Service에 현재 방 탈퇴 메세지 요청
            bool result = await _chatService.LeaveRoomAsync(CurrentRoom.RoomId);
            if (!result) return;

            Guid leftRoomId = CurrentRoom.RoomId;
            CurrentRoom = null;

            // 3. TODO: ChatListView에게 현재 입장한 방이 삭제됐음을 알려야 함 (ChatHub 탈퇴도 List에서 해야할듯)
        }
        #endregion
        #region OnChanged
        /// <summary>
        /// CurrentRoom이 변경되면 호출합니다.
        /// </summary>
        /// <param name="value">변경 값</param>
        partial void OnCurrentRoomChanged(ChatRoomDetailModel? value)
        {
            if (value == null) return;
            IsSidePanelVisible = false;
        }
        /// <summary>
        /// CurrentRoom이 변경되기 직전에 호출됩니다.
        /// </summary>
        /// <param name="value">변경되기 이전의 값</param>
        partial void OnCurrentRoomChanging(ChatRoomDetailModel? value)
        {
            if (value == null) return;
            _ = Task.Run(async () => await _chatHubService.LeaveRoomAsync(value.RoomId));
        }
        /// <summary>
        /// 서버로부터 현재 접속중인 방에 새로운 메세지가 도착했을때 실행되는 메서드
        /// </summary>
        /// <param name="response"></param>
        private void OnMessageReceived(ChatMessageResponse response)
        {
            // 1. 현재 방의 메시지인지 확인
            if (response == null || _identityService.MyProfile == null
                || CurrentRoom == null || CurrentRoom.RoomId != response.RoomId)
                return;

            // 2. Message List에 추가
            App.Current.Dispatcher.Invoke(async () =>
            {
                ChatMessageModel newMessage = new(response, _identityService.MyProfile.Email);
                CurrentRoom.AddMessage(newMessage);

                // 3. 내가 보낸 메세지가 아니면 읽음 처리 호출
                if (!newMessage.IsMine)
                {
                    await UpdateLastReadedMessage(response.RoomId, response.MessageId);
                }
            });
        }
        /// <summary>
        /// 누군가가 현재 방의 메세지를 읽었을때, ChatHubService로 서버가 신호를 보내는데, 이를 감지하여 동작하는 메서드입니다.
        /// </summary>
        /// <remarks>
        /// 누군가 특정 메세지까지 읽었으니 해당 메세지 번호보다 작은 메세지들의 UnreadPeopleCount 값을 -- 처리합니다.
        /// </remarks>
        /// <param name="response"></param>
        private void OnReadStatusUpdated(UserReadUpdateResponse response)
        {
            // 1. 현재 방인지 확인
            if (CurrentRoom == null || CurrentRoom.RoomId != response.RoomId) return;
            if (_identityService.MyProfile?.Email == response.UserEmail) return;
            // 2. 해당 ID 이하의 메시지들 카운트 감소
            UpdateMessagesReadStatus(response.LastReadMessageId, response.PreviousLastReadMessageId);
        }
        #endregion
        #region private Method
        /// <summary>
        /// 메세지 혹은 이벤트들을 구독합니다
        /// </summary>
        private void SubscribeEvents()
        {
            // Server가 ChatHubService에게 신호를 보내면 ViewModel이 감지하여 특정 메서드를 실행하게 합니다.
            _chatHubService.MessageReceivedEvent += OnMessageReceived;
            _chatHubService.ReadStatusUpdatedEvent += OnReadStatusUpdated;
        }
        /// <summary>
        /// 채팅방의 상세 정보를 읽어오고, 실시간 채팅자로 입장합니다.
        /// </summary>
        /// <param name="roomId">채팅방의 식별 번호</param>
        private async Task LoadRoomDetailAsync(Guid roomId)
        {
            if (_identityService.MyProfile == null) return;
            // 1. 입장하려는 방의 상세 정보를 가져옵니다.
            ChatRoomDetailModel? response = await _chatService.GetChatRoomDetailAsync(roomId, _identityService.MyProfile.Email);
            if (response == null) return;
            CurrentRoom = response;

            // 2. 현재 방에 메세지가 하나라도 있으면 마지막 메세지를 가져옵니다.
            ChatMessageModel? lastMessage = CurrentRoom.SortedMessages.Cast<ChatMessageModel>().LastOrDefault();
            if (lastMessage == null) return;

            // 3. 채팅방 정보에 등록된 LastReadMessageId가 실제 마지막 메세지의 Id보다 값이 작으면
            // 메세지를 수신했다고 서버에 신호를 보냅니다.
            if (lastMessage.MessageId > CurrentRoom.LastReadMessageId)
            {
                await UpdateLastReadedMessage(roomId, lastMessage.MessageId);
            }

            // 4. 채팅방에 입장해 정보를 실시간으로 받아옵니다.
            await _chatHubService.JoinRoomAsync(roomId);
        }
        /// <summary>
        /// 현재 방의 특정 메세지를 읽었으니 업데이트하라고 서버에게 요청합니다.
        /// </summary>
        /// <remarks>
        /// 채팅방에 입장했거나, 실시간 통신에서 메세지를 수신했을때,<br/>
        /// 마지막 메세지를 읽었다고 서버에게 업데이트 요청을 보내고 정상적으로 처리됐다는 신호를 받으면 메모리 값도 변경합니다.
        /// </remarks>
        /// <param name="roomId">메세지를 읽은 방의 식별 번호</param>
        /// <param name="messageId">읽은 메세지의 식별 번호</param>
        private async Task UpdateLastReadedMessage(Guid roomId, long messageId)
        {
            // 1. 마지막으로 읽은 메세지 Update 요청용 request 객체 생성
            UpdateLastReadedMessageRequest request = new()
            {
                RoomId = roomId,
                LastReadMessageId = messageId
            };
            // 2. 마지막으로 읽은 메세지 Update 요청
            bool result = await _chatService.UpdateLastReadedMessageAsync(request);
            if (!result || CurrentRoom == null) return;
            // 3. 서버로부터의 반환값이 true면 메모리 값 수정
            UpdateMessagesReadStatus(messageId, CurrentRoom.LastReadMessageId);
            CurrentRoom.LastReadMessageId = messageId;
            CurrentRoom.UnreadCount = 0;
            // 4. ChatListViewModel에게도 현재 방의 UnreadCount를 0으로 변경하라고 신호 전송
            WeakReferenceMessenger.Default.Send(new ChatRoomReadMarkedMessage(roomId));
        }
        private void UpdateMessagesReadStatus(long lastMessageId, long previousLastMessageId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // 최근 메세지부터 조회하기위해 메세지들을 역순으로 가져옴
                IEnumerable<ChatMessageModel>? messages = CurrentRoom?.SortedMessages.Cast<ChatMessageModel>().Reverse();
                if (messages == null) return;

                foreach (ChatMessageModel message in messages)
                {
                    // 전달받은 messageId보다 Id가 큰 메세지는 아직 처리 대상이 아니므로 건너뜀
                    if (message.MessageId > lastMessageId) continue;
                    // 이전에 읽었던 마지막 메세지 Id보다 message의 Id가 낮으면 이미 카운트를 줄인 메세지이므로 break
                    if (previousLastMessageId >= message.MessageId) break;
                    // message의 unreadpeoplecount가 0이거나 음수면 뒤 메세지도 이미 처리됐을테니 break
                    if (0 >= message.UnreadPeopleCount) break;

                    // 메세지의 UnreadPeopleCount 감소
                    message.UnreadPeopleCount--;
                }
            });
        }
        #endregion
    }
}
