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
        /// <param name="roomId">채팅방 식별 번호</param>
        public async void SetChatRoom(Guid roomId)
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

            // 1. TODO: 진짜 채팅방 나갈것인지 확인 입력 받아야함

            // 2. Service에 현재 방 탈퇴 메세지 요청
            ServiceResult<bool> result = await _chatService.LeaveRoomAsync(CurrentRoom.RoomId);
            if (!result.IsSuccess) return;

            Guid leftRoomId = CurrentRoom.RoomId;
            await CloseCurrentRoom();

            // 3. ChatListView에게 현재 입장한 방이 삭제됐음을 알림
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
                // 3. 내가 보낸 메세지가 아니면 읽음 처리 호출 (내가 메세지를 전송하면 나의 lastReadedMessagId는 자동으로 전송한 MessageId로 업데이트됨)
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
        /// 누군가 특정 메세지까지 읽었으니 해당 메세지 번호보다 작은 메세지들의 UnreadPeopleCount 값을 -- 처리합니다.
        /// </remarks>
        /// <param name="response"></param>
        private void OnReadStatusUpdated(UserReadUpdateResponse response)
        {
            // 1. 현재 방인지 확인
            if (CurrentRoom == null || CurrentRoom.RoomId != response.RoomId) return;
            // 2. 내가 보낸 메세지는 전송 이후 처리했기때문에 return
            if (_identityService.MyProfile.Email == response.UserEmail) return;
            // 2. 해당 ID 이하의 메시지들 카운트 감소
            DecrementUnreadCounts(response.LastReadMessageId, response.PreviousLastReadMessageId);
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
                ProcessIncomingMessage(response.Message);
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
            // 3. 서버로부터의 반환값이 true면 메모리 값 수정
            DecrementUnreadCounts(messageId, CurrentRoom.LastReadMessageId);
            // 4. CurrentRoom messageId까지 읽음 처리
            CurrentRoom.MarkAsRead(messageId);
            // 5. ChatListViewModel에게도 현재 방의 UnreadCount를 0으로 변경하라고 신호 전송
            WeakReferenceMessenger.Default.Send(new ChatRoomReadMarkedMessage(roomId));
        }
        /// <summary>
        /// 특정 메세지의 UnreadPeopleCount를 1만큼 감소시킵니다
        /// </summary>
        /// <remarks>
        /// lastMessageId와 previouseLastMessageId 사이에 존재하는 메세지들의 UnreadPeopleCount를 1씩 감소시킵니다.
        /// </remarks>
        /// <param name="lastMessageId">UnreadPeopleCount를 감소시키기 시작할 메세지의 Id</param>
        /// <param name="previousLastMessageId">UnreadPeopleCount를 감소시키고 메서드 종료할 메세지의 Id</param>
        private void DecrementUnreadCounts(long lastMessageId, long previousLastMessageId)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                // 최근 메세지부터 조회하기위해 메세지들을 역순으로 가져옴
                IEnumerable<ChatMessageModel>? messages = CurrentRoom?.Messages.Reverse();
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
                if(isJoined)
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
