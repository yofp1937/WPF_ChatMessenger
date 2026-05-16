using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages.Tab.Chat;
using ChatMessenger.Client.Models.Chats;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ChatMessenger.Client.ViewModels.Tabs.Chats
{
    public partial class CreateChatRoomViewModel : BaseViewModel
    {
        private IFriendService _friendService;
        private IChatService _chatService;

        // 방 제목
        [ObservableProperty]
        private string _roomTitle = string.Empty;
        [ObservableProperty]
        private string _warningText = string.Empty;

        // 친구 목록
        [ObservableProperty]
        private ObservableCollection<FriendModel> _friendList = new();

        public CreateChatRoomViewModel(IFriendService friendService, IChatService chatService)
        {
            _friendService = friendService;
            _chatService = chatService;
        }

        /// <summary>
        /// 입력된 값들을 초기화합니다.
        /// </summary>
        public async Task ResetInputValues()
        {
            RoomTitle = string.Empty;
            WarningText = string.Empty;
            // 초대 가능한 친구 목록 채우기
            await LoadFriendsAsync();
        }
        #region RelayCommand
        /// <summary>
        /// 초대 버튼이 클릭되면 동작합니다.
        /// </summary>
        /// <remarks>
        /// FriendList에서 IsSelected가 true인 User만 골라서 새로운 채팅방을 생성합니다.
        /// </remarks>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanCreateChatRoom))]
        private async Task CreateChatRoom()
        {
            // 1. User가 선택한 친구들 Email 추출
            List<string>? emails = FriendList
                .Where(f => f.IsSelected)
                .Select(f => f.Email)
                .ToList();
            if (emails == null || emails.Count == 0) return;

            // 2. 인원수에 맞는 채팅방 생성 메서드 호출하고 roomId 받아옴
            ChatRoomSummaryModel? roomModel = emails.Count == 1
                ? await _chatService.CreatePrivateChatRoomAsync(emails.First())
                : await _chatService.CreateGroupChatAsync(RoomTitle, null, emails);
            if (roomModel == null) return;

            // 3. ChatRoom 화면으로 전환 요청
            WeakReferenceMessenger.Default.Send(new ChatRoomSelectionChangedMessage(roomModel.RoomId));
            // 4. ChatListVM에 새로 생성된 방 추가
        }
        #endregion RelayCommand
        #region OnChanged Method
        /// <summary>
        /// 사용자가 RoomTitle TextBox를 통해 제목을 입력하면 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 0.35초간 데이터 입력이 없을때만 친구 목록 정렬을 명령합니다.
        /// </remarks>
        partial void OnRoomTitleChanged(string value)
        {
            if(string.IsNullOrWhiteSpace(value))
            {
                WarningText = string.Empty;
                return;
            }

            // 글자 수 검증
            if (4 > value.Length)
                WarningText = "방 제목은 최소 4글자 이상이어야 합니다.";
            else if (value.Length > 24)
                WarningText = "방 제목은 최대 24글자까지 가능합니다.";
            else
                // 정상 범위일 경우 경고 지움
                WarningText = string.Empty;

            // 글자가 변경될때마다 CreateChatRoomCommand의 CanExecute 갱신하도록 이벤트 등록
            CreateChatRoomCommand.NotifyCanExecuteChanged();
        }
        #endregion OnChanged Method
        #region private Method
        /// <summary>
        /// 메서드가 실행되기전에 조건에 부합하는지 확인합니다.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateChatRoom()
        {
            // 방 제목이 4~24글자 사이고, IsSelected가 true인게 하나라도 존재하면 true
            return RoomTitle.Length > 3 && 25 > RoomTitle.Length && FriendList.Any(f => f.IsSelected);
        }
        /// <summary>
        /// 서버로부터 친구 목록을 받아옵니다.
        /// </summary>
        /// <returns></returns>
        private async Task LoadFriendsAsync()
        {
            try
            {
                // FriendListViewModel의 LoadFriendsAsync와 동일한 서비스 호출
                List<FriendModel>? result = await _friendService.GetFriendsListAsync();
                if (result == null || result.Count == 0) return;

                FriendList.Clear();
                foreach (FriendModel friend in result)
                {
                    // IsSelected 상태가 변경되면 CreateChatRoomCommand의 CanExecute 갱신하도록 이벤트 등록
                    friend.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(FriendModel.IsSelected))
                        {
                            CreateChatRoomCommand.NotifyCanExecuteChanged();
                        }
                    };
                    FriendList.Add(friend);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(CreateChatRoomViewModel)}_{nameof(LoadFriendsAsync)}]  Error: {ex.Message}");
            }
        }
        #endregion private Method
    }
}
