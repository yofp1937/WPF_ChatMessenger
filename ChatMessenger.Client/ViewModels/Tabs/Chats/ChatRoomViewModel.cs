using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages.Tab.Chat;
using ChatMessenger.Client.Common.Messages.Tab.Chat.Room;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Chat;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

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

        #region 생성자, override
        public ChatRoomViewModel(IIdentityService identityService, IChatService chatService, IChatHubService chatHubService)
        {
            _identityService = identityService;
            _chatService = chatService;
            _chatHubService = chatHubService;

            Subscribe();
        }
        /// <inheritdoc/>
        protected override void Subscribe()
        {
            // Server가 ChatHubService에게 신호를 보내면 ViewModel이 감지하여 특정 메서드를 실행하게 합니다.
            _chatHubService.MessageReceivedEvent += OnMessageReceived;
            _chatHubService.ReadStatusUpdatedEvent += OnReadStatusUpdated;
            _chatHubService.UpdateParticipantStatusEvent += OnParticipantStatusUpdated;
        }
        /// <inheritdoc/>
        /// <remarks>
        /// ChatHubService의 Action이벤트 구독을 해제합니다.
        /// </remarks>
        public override void CleanUp()
        {
            base.CleanUp();
            _chatHubService.MessageReceivedEvent -= OnMessageReceived;
            _chatHubService.ReadStatusUpdatedEvent -= OnReadStatusUpdated;
            _chatHubService.UpdateParticipantStatusEvent -= OnParticipantStatusUpdated;
            CurrentRoom = null;
        }
        #endregion 생성자, override
        #region public Method
        /// <summary>
        /// 화면을 roomId 채팅방 화면으로 변경하고 입장합니다.
        /// </summary>
        /// <remarks>
        /// async void 대신 async Task로 선언하여, 내부에서 예외가 발생했을 때 호출부에서 Task를 통해 예외를 관측할 수 있게 합니다.<br/>
        /// async void는 예외가 발생해도 호출부로 전파되지 않고 애플리케이션이 곧바로 종료(크래시)되는 문제가 있습니다.
        /// </remarks>
        /// <param name="roomId">채팅방 식별 번호</param>
        public async Task SetChatRoom(Guid roomId)
        {
            await LoadRoomDetailAsync(roomId);
        }
        #endregion public Method
        #region RelayCommand
        /// <summary>
        /// 채팅방 오른쪽 정보 창을 열거나 닫습니다.
        /// </summary>
        [RelayCommand]
        private void ToggleSidePanel()
        {
            IsSidePanelVisible = !IsSidePanelVisible;
        }
        /// <summary>
        /// 현재 채팅방 화면을 닫습니다.
        /// </summary>
        [RelayCommand]
        private async Task CloseCurrentRoom()
        {
            if (CurrentRoom == null)
                return;
            await _chatHubService.LeaveRoomAsync(CurrentRoom.RoomId);
            CurrentRoom = null;
            // ChatListViewModel의 SelectRoom과 CurrentRoom을 동기화하기위해 메세지 전송
            WeakReferenceMessenger.Default.Send(new ChatRoomClosedMessage());
        }
        /// <summary>
        /// InputMessage TextBox에 입력된 내용을 채팅방에 전송합니다.
        /// </summary>
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
        private void InviteFriend()
        {
            if (CurrentRoom == null) return;
            // 내 친구의 Eamil과 채팅방 참여자의 Email이 일치하면 해당 친구 초대 불가능하게 만들기위해 추출
            List<string> existingEmails = CurrentRoom.Participants.Select(p => p.Email).ToList();
            // 1. 그룹 채팅일때 친구 초대하면 참가자 정보 포함해서 Message 전송
            if (CurrentRoom.IsGroupChat)
            {
                WeakReferenceMessenger.Default.Send(new OpenCreateChatRoomRequestMessage(CurrentRoom.RoomId, CurrentRoom.Title, CurrentRoom.RoomProfileImageURL, existingEmails));
            }
            // 2. 1대1 채팅이면 기본 그룹
            else
            {
                // 신규 그룹채팅 생성이므로 RoomId는 null로 Message 전송
                WeakReferenceMessenger.Default.Send(new OpenCreateChatRoomRequestMessage(null, null, null, existingEmails));
            }
        }
        [RelayCommand]
        private async Task LeaveRoom()
        {
            if (CurrentRoom == null) return;

            // 1. Service에 현재 방 탈퇴 메세지 요청
            ServiceResult<bool> result = await _chatService.LeaveRoomAsync(CurrentRoom.RoomId);
            if (!result.IsSuccess) return;

            Guid leftRoomId = CurrentRoom.RoomId;
            await CloseCurrentRoom();

            // 2. ChatListView에게 현재 입장한 방이 삭제됐음을 알림
            WeakReferenceMessenger.Default.Send(new LeaveChatRoomMessage(leftRoomId));
        }
        #endregion RelayCommand
        #region OnChanged
        /// <summary>
        /// CurrentRoom이 변경되면 호출합니다.
        /// </summary>
        /// <param name="value">변경 값</param>
        partial void OnCurrentRoomChanged(ChatRoomDetailModel? value)
        {
            IsSidePanelVisible = false;
            if (value == null)
                return;
            _ = Task.Run(async () => await _chatHubService.JoinRoomAsync(value.RoomId));
        }
        #endregion OnChanged
        #region private Method
        #region ChatHub Action Event와 연결된 Method
        /// <summary>
        /// 서버로부터 현재 접속중인 방에 새로운 메세지가 도착했을때 실행되는 메서드
        /// </summary>
        /// <param name="response"></param>
        private void OnMessageReceived(ChatMessageResponse response)
        {
            // 1. 현재 방의 메시지인지 확인
            if (response == null || CurrentRoom == null || CurrentRoom.RoomId != response.RoomId)
                return;

            // 2. Message List에 추가
            App.Current.Dispatcher.Invoke(async () =>
            {
                ChatMessageModel newMessage = ProcessIncomingMessage(response);
                // 3. 발신자는 자기 메세지를 읽은 상태이므로 읽은 위치를 갱신 (이후 재계산에서 발신자가 미읽음으로 집계되지 않도록)
                if (response.Sender != null)
                    CurrentRoom.UpdateReadPosition(response.Sender.Email, response.MessageId);
                // 4. 내가 보낸 메세지가 아니면 읽음 처리 호출 (내가 메세지를 전송하면 나의 lastReadedMessagId는 자동으로 전송한 MessageId로 업데이트됨)
                if (!newMessage.IsMine)
                {
                    await UpdateLastReadedMessageAsync(response.RoomId, response.MessageId);
                }
            });
        }
        /// <summary>
        /// 누군가가 현재 방의 메세지를 읽었을때, ChatHubService로 서버가 신호를 보내는데, 이를 감지하여 동작하는 메서드입니다.
        /// </summary>
        /// <remarks>
        /// 해당 참가자의 읽은 위치를 갱신한 뒤, 모든 메세지의 UnreadPeopleCount를 위치 집합에서 다시 계산합니다.
        /// </remarks>
        /// <param name="response"></param>
        private void OnReadStatusUpdated(UserReadUpdateResponse response)
        {
            // 1. 현재 방인지 확인
            if (CurrentRoom == null || CurrentRoom.RoomId != response.RoomId) return;
            // 2. 내 읽음은 UpdateLastReadedMessageAsync에서 이미 처리했으므로 return
            if (_identityService.MyProfile.Email == response.UserEmail) return;
            // 3. 상대방의 읽은 위치를 갱신하고 안 읽은 사람 수를 파생 재계산
            CurrentRoom.UpdateReadPosition(response.UserEmail, response.LastReadMessageId);
            CurrentRoom.RecalculateUnreadCounts();
        }
        /// <summary>
        /// 현재 방의 참가자가 변경 메세지 수신시 호출되는 메서드  
        /// </summary>
        /// <param name="response"></param>
        private void OnParticipantStatusUpdated(ChatParticipantStatusResponse response)
        {
            if (CurrentRoom == null || CurrentRoom.RoomId != response.Message.RoomId) return;

            App.Current.Dispatcher.Invoke(() =>
            {
                CurrentRoom.ParticipantCount = response.CurrentParticipantCount;
                UpdateParticipantList(response.TargetUsers, response.IsJoined);
                // 입퇴장에 맞춰 읽은 위치 맵도 갱신한다.
                // 입장자는 진입 시점(입장 시스템 메세지) 위치로 등록되어 이전 메세지 카운트에 영향을 주지 않고,
                // 퇴장자는 집계 대상에서 제거되어 남은 메세지의 안 읽은 사람 수가 서버 기준과 일치한다.
                foreach (FriendResponse user in response.TargetUsers)
                {
                    if (response.IsJoined)
                        CurrentRoom.UpdateReadPosition(user.Email, response.Message.MessageId);
                    else
                        CurrentRoom.RemoveReadPosition(user.Email);
                }
                ProcessIncomingMessage(response.Message);
                CurrentRoom.RecalculateUnreadCounts();
            });
        }
        #endregion ChatHub Action Event와 연결된 Method
        /// <summary>
        /// 채팅방의 상세 정보를 읽어오고, 실시간 채팅자로 입장합니다.
        /// </summary>
        /// <param name="roomId">채팅방의 식별 번호</param>
        private async Task LoadRoomDetailAsync(Guid roomId)
        {
            if (_identityService.MyProfile == null) return;
            // 1. 입장하려는 방의 상세 정보를 가져옵니다.
            ServiceResult<ChatRoomDetailModel> response = await _chatService.GetChatRoomDetailModelAsync(roomId, _identityService.MyProfile.Email);
            if (!response.IsSuccess) return;
            CurrentRoom = response.Data;

            // 2. 현재 방에 메세지가 하나라도 있으면 마지막 메세지를 가져옵니다.
            ChatMessageModel? lastMessage = CurrentRoom.Messages.LastOrDefault();
            if (lastMessage == null) return;

            // 3. 채팅방 정보에 등록된 LastReadMessageId가 실제 마지막 메세지의 Id보다 값이 작으면
            // 메세지를 수신했다고 서버에 신호를 보냅니다.
            if (lastMessage.MessageId > CurrentRoom.LastReadMessageId)
            {
                await UpdateLastReadedMessageAsync(roomId, lastMessage.MessageId);
            }
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
        private async Task UpdateLastReadedMessageAsync(Guid roomId, long messageId)
        {
            // 1. 마지막으로 읽은 메세지 Update 요청용 request 객체 생성
            UpdateLastReadedMessageRequest request = new()
            {
                RoomId = roomId,
                LastReadMessageId = messageId
            };
            // 2. 마지막으로 읽은 메세지 Update 요청
            ServiceResult<bool> result = await _chatService.UpdateLastReadedMessageAsync(request);
            if (!result.IsSuccess || CurrentRoom == null) return;
            // 3. 내 읽은 위치를 갱신하고 안 읽은 사람 수를 파생 재계산
            CurrentRoom.UpdateReadPosition(_identityService.MyProfile.Email, messageId);
            CurrentRoom.RecalculateUnreadCounts();
            // 4. CurrentRoom messageId까지 읽음 처리 (내 UnreadCount / LastReadMessageId 갱신)
            CurrentRoom.MarkAsRead(messageId);
            // 5. ChatListViewModel에게도 현재 방의 UnreadCount를 0으로 변경하라고 신호 전송
            WeakReferenceMessenger.Default.Send(new ChatRoomReadMarkedMessage(roomId));
        }
        /// <summary>
        /// ChatMessageResponse를 ChatMessageModel로 변환하고 CurrentRoom의 메세지 목록에 추가하는 공통 로직
        /// </summary>
        /// <param name="response">메세지 전송 DTO</param>
        /// <returns>생성된 메세지 모델</returns>
        private ChatMessageModel ProcessIncomingMessage(ChatMessageResponse response)
        {
            ChatMessageModel newMessage = new(response, _identityService.MyProfile.Email);
            CurrentRoom?.AddMessage(newMessage);
            return newMessage;
        }
        /// <summary>
        /// 리스트 형태의 참가자 정보로 현재 방의 참여자 목록을 업데이트합니다.
        /// </summary>
        /// <param name="users">방에 참가하거나 퇴장한 User들의 FriendResponse List</param>
        /// <param name="isJoined">참가 처리시 true, 퇴장 처리시 false</param>
        private void UpdateParticipantList(IEnumerable<FriendResponse> users, bool isJoined)
        {
            if (CurrentRoom == null) return;
            foreach (FriendResponse user in users)
            {
                string x = "님 CurrentRoom.Participants에";
                x += isJoined ? " 등록" : "서 삭제";
                if (isJoined)
                {
                    // 새로운 참가자 Email과 동일한 Email을 사용하는 유저가 없으면 추가
                    if (!CurrentRoom.Participants.Any(p => p.Email == user.Email))
                        CurrentRoom.Participants.Add(new FriendModel(user));
                }
                else
                {
                    // 탈퇴자와 동일한 Email을 사용하는 유저 제거
                    FriendModel? target = CurrentRoom.Participants.FirstOrDefault(p => p.Email == user.Email);
                    if (target != null)
                        CurrentRoom.Participants.Remove(target);
                }
            }
        }
        #endregion
    }
}
