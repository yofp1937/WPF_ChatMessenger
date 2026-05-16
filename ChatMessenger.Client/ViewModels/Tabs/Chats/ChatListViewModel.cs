using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.Common.Messages.Tab.Chat;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.DTOs.Responses.Chat;
using ChatMessenger.Shared.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

namespace ChatMessenger.Client.ViewModels.Tabs.Chats
{
    public partial class ChatListViewModel : BaseViewModel
    {
        private readonly IChatService _chatService;
        private readonly IChatHubService _chatHubService;

        // 채팅방 검색과 Binding된 string
        [ObservableProperty]
        private string? _searchRoomText = string.Empty;
        // 채팅방 검색에서 0.35초간 데이터 입력이 없어야 검색 시도하게 만들어주는 토큰
        private CancellationTokenSource? _searchCts;

        private ObservableCollection<ChatRoomSummaryModel> _rooms = new();
        // UI와 Binding될 채팅방 목록
        public ICollectionView ChatRooms { get; }
        [ObservableProperty]
        private ChatRoomSummaryModel? _selectedRoom;

        public ChatListViewModel(IChatService chatService, IChatHubService chatHubService)
        {
            _chatService = chatService;
            _chatHubService = chatHubService;

            Subscribe();

            // View와 Binding될 채팅방 목록 생성, 정렬과 필터 적용
            ChatRooms = CollectionViewSource.GetDefaultView(_rooms);
            InitializeChatRoomsSetting();

            // 채팅방 목록 불러오기
            _ = GetMyChatRoomsAsync();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// ChatHub 이벤트 구독도 해제합니다.
        /// </remarks>
        public override void CleanUp()
        {
            base.CleanUp();
            _chatHubService.MessageReceivedEvent -= OnMessageReceived;
            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }

        #region OnChanged Method
        /// <summary>
        /// 사용자가 Search TextBox를 통해 채팅방 검색을 시도하면 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 0.35초간 데이터 입력이 없을때만 채팅방 목록 정렬을 명령합니다.
        /// </remarks>
        partial void OnSearchRoomTextChanged(string? value)
        {
            // 기존 요청 취소
            _searchCts?.Cancel();
            _searchCts = new();
            CancellationToken token = _searchCts.Token;

            // 비동기로 0.35초 대기 후 실행 명령
            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(350, token);
                    if (App.Current == null || token.IsCancellationRequested) return;
                    // 0.35초 지났으면 UI 스레드에서 필터 적용
                    await App.Current.Dispatcher.InvokeAsync(() => ChatRooms.Refresh());
                }
                catch (TaskCanceledException) { /* Token 취소됐을때 실행 (무시) */ }
            });
        }

        /// <summary>
        /// 사용자가 ListBox에서 채팅방을 선택하면 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 채팅방이 선택되면 채팅방의 RoomId를 ChatRoomViewModel로 넘기고 ChatRoomViewModel에서 해당 방의 상세 정보를 읽어옵니다.
        /// </remarks>
        /// <param name="value"></param>
        partial void OnSelectedRoomChanged(ChatRoomSummaryModel? value)
        {
            if (value == null) return;
            WeakReferenceMessenger.Default.Send(new ChatRoomSelectionChangedMessage(value.RoomId));
        }

        /// <summary>
        /// 서버로부터 메세지를 수신했을때 목록의 정보를 최신화합니다.
        /// </summary>
        private void OnMessageReceived(ChatMessageResponse response)
        {
            ChatRoomSummaryModel? room = _rooms.FirstOrDefault(r => r.RoomId == response.RoomId);
            if (room == null)
            {
                // 목록에 없는 방에서 메세지가 도착했다면 신규 방에 초대된것
                _ = GetMyChatRoomsAsync();
            }
            else
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    room.LastMessage = response.Content;
                    room.LastMessageSentAt = response.SentAt.ToLocalTime();
                    if (SelectedRoom?.RoomId != response.RoomId && response.MessageType != ChatMessageType.System)
                    {
                        room.UnreadCount++;
                    }
                    ChatRooms.Refresh();
                });
            }
        }
        #endregion
        #region RelayCommand
        [RelayCommand]
        private void CreateChatRoom()
        {
            WeakReferenceMessenger.Default.Send(new OpenCreateChatRoomRequestMessage());
        }
        #endregion
        #region private Method
        /// <summary>
        /// 메세지 혹은 이벤트를 구독합니다.
        /// </summary>
        private void Subscribe()
        {
            // ChatRoomViewModel의 CurrentRoom이 닫힐때 SelectedRoom도 null로 동기화 시키기위함
            WeakReferenceMessenger.Default.Register<ChatRoomClosedMessage>(this, (r, m) =>
            {
                SelectedRoom = null;
            });
            // ChatRoomViewModel에서 메세지를 수신해서 읽었으면 해당 방의 UnreadCount도 동기화 시키기위함
            WeakReferenceMessenger.Default.Register<ChatRoomReadMarkedMessage>(this, (r, m) =>
            {
                ChatRoomSummaryModel? room = _rooms.FirstOrDefault(x => x.RoomId == m.roomId);
                if (room == null) return;

                App.Current.Dispatcher.Invoke(() =>
                {
                    room.UnreadCount = 0;
                    ChatRooms.Refresh();
                });
            });
            // 새로운 채팅방이 만들어졌을때 _rooms에 등록시키기 위함
            WeakReferenceMessenger.Default.Register<NewChatRoomCreatedMessage>(this, (r, m) =>
            {

            });
            _chatHubService.MessageReceivedEvent += OnMessageReceived;
        }
        /// <summary>
        /// 사용자의 채팅방 목록을 불러옵니다.
        /// </summary>
        private async Task GetMyChatRoomsAsync()
        {
            try
            {
                List<ChatRoomSummaryModel>? chatRoomList = await _chatService.GetMyChatRoomsAsync();
                if (chatRoomList == null) return;

                // UI 스레드에서 room 리스트 업데이트
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    _rooms.Clear();
                    foreach (ChatRoomSummaryModel room in chatRoomList)
                        _rooms.Add(room);
                    ChatRooms.Refresh();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{GetType().ToString()} - GetMyChatRoomsAsync]: 초기 로드 실패: {ex.Message}");
            }
        }

        /// <summary>
        /// 채팅방 목록에 정렬과 필터링을 설정합니다.
        /// </summary>
        private void InitializeChatRoomsSetting()
        {
            // 1. 필터 조건 설정
            ChatRooms.Filter = roomObj =>
            {
                if (roomObj is not ChatRoomSummaryModel room) return false;
                if (string.IsNullOrWhiteSpace(SearchRoomText)) return true;

                return room.Title.Contains(SearchRoomText, StringComparison.OrdinalIgnoreCase);
            };
            // 2. 정렬 조건 설정
            ChatRooms.SortDescriptions.Clear(); // 중복 추가 방지
            // 우선순위 1: 읽지 않은 메세지 여부 기준(숫자가 큰순 - 내림차순)
            ChatRooms.SortDescriptions.Add(new SortDescription("HasUnreadMessages", ListSortDirection.Descending));
            // 우선순위 2: 마지막 메시지 시간 기준 (최신순 - 내림차순)
            ChatRooms.SortDescriptions.Add(new SortDescription("LastMessageSentAt", ListSortDirection.Descending));
            // 우선순위 3: 시간이 같을 경우 제목 기준 (오름차순)
            ChatRooms.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));
        }
        #endregion
    }
}
