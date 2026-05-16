using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.Common.Messages.Tab.Friend;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace ChatMessenger.Client.ViewModels.Tabs.Friends
{
    public partial class FriendListViewModel : BaseViewModel
    {
        private readonly IIdentityService _identityService;
        private readonly IFriendService _friendService;

        // 로그인한 사용자의 프로필
        [ObservableProperty]
        private FriendModel? _userProfile;

        // 로그인한 사용자가 추가한 친구 목록
        private ObservableCollection<FriendModel> _friends = new();
        // View에 Binding되어 보여질 친구 목록
        public ICollectionView FriendsList { get; }

        // 친구 검색과 Binding된 string
        [ObservableProperty]
        private string? _searchNickname = string.Empty;
        // 친구 검색에서 0.35초간 데이터 입력이 없어야 검색 시도하게 만들어주는 토큰
        private CancellationTokenSource? _searchCts;
        // 오른쪽 Panel에 정보를 표시할 선택된 친구 Data
        [ObservableProperty]
        private FriendModel? _selectedFriend = null;


        public FriendListViewModel(IIdentityService identityService, IFriendService friendService)
        {
            SubscribeMessages();
            _identityService = identityService;
            _friendService = friendService;

            // View와 Binding될 친구 목록 생성, 정렬과 필터 적용
            FriendsList = CollectionViewSource.GetDefaultView(_friends);
            InitializeFriendsListSetting();

            // 로그인에 성공했지만 Profile 정보가 존재하지않으면 강제 로그아웃
            if (_identityService.MyProfile == null)
            {
                WeakReferenceMessenger.Default.Send(new ForceLogoutMessage());
                return;
            }
            _userProfile = _identityService.MyProfile;

            // 친구 목록 불러오기
            _ = LoadFriendsAsync();
        }
        /// <inheritdoc/>
        public override void CleanUp()
        {
            base.CleanUp();
            _searchCts?.Cancel();
            _searchCts?.Dispose();
        }
        #region RelayCommand
        /// <summary>
        /// 서버로부터 친구 목록을 불러와 Friends Property에 채웁니다.
        /// </summary>
        [RelayCommand] // 나중에 친구 목록 새로고침 기능이 추가될수있으니 RelayCommand로 선언해둠
        private async Task LoadFriendsAsync()
        {
            List<FriendModel>? result = await _friendService.GetFriendsListAsync();
            if (result == null || result.Count == 0) return;

            _friends.Clear();
            foreach (FriendModel friend in result)
                _friends.Add(friend);
        }
        /// <summary>
        /// FriendDetailView에 친구 추가 Panel을 표시하라고 Messenger를 통해 전달합니다.
        /// </summary>
        [RelayCommand]
        private void ChangeAddFriendMode()
        {
            WeakReferenceMessenger.Default.Send(new AddFriendModeChangedMessage());
        }
        #endregion
        #region OnChanged Method
        /// <summary>
        /// 사용자가 Search TextBox를 통해 친구 검색을 시도하면 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 0.35초간 데이터 입력이 없을때만 친구 목록 정렬을 명령합니다.
        /// </remarks>
        partial void OnSearchNicknameChanged(string? value)
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
                    await App.Current.Dispatcher.InvokeAsync(() => FriendsList.Refresh());
                }
                catch (TaskCanceledException) { /* Token 취소됐을때 실행 (무시) */ }
            });
        }
        /// <summary>
        /// SelectedFriend 프로퍼티의 값이 바뀔때 자동으로 호출되는 메서드
        /// </summary>
        /// <remarks>
        /// ListBox에서 자신의 프로필 혹은 친구를 선택시<br/>
        /// 오른쪽 FriendDetailView에 상세 정보를 띄우기위해 FriendDetailViewModel에게 데이터를 전송합니다.
        /// </remarks>
        /// <param name="value"></param>
        partial void OnSelectedFriendChanged(FriendModel? value)
        {
            if (value == null) return;

            WeakReferenceMessenger.Default.Send(new FriendSelectionChangedMessage(value));
        }
        #endregion
        #region private Method
        private void SubscribeMessages()
        {
            WeakReferenceMessenger.Default.Register<SelectedFriendResetMessage>(this, (r, m) =>
            {
                SelectedFriend = null;
            });
            WeakReferenceMessenger.Default.Register<FriendAddedMessage>(this, (r, m) =>
            {
                // 1.중복 데이터가 존재하면 추가 안함
                if (_friends.Any(f => f.Email == m.friend.Email)) return;
                // 2.원본 친구 리스트에 추가하고 정렬
                _friends.Add(m.friend);
            });
            WeakReferenceMessenger.Default.Register<FriendDeletedMessage>(this, (r, m) =>
            {
                // 1.친구 목록에 전달받은 Email의 친구 있는지 확인
                FriendModel? target = _friends.FirstOrDefault(f => f.Email == m.friend.Email);
                if (target == null) return;
                // 2.있으면 삭제하고 정렬
                _friends.Remove(target);
            });
            WeakReferenceMessenger.Default.Register<FriendStatusChangeMessage>(this, (r, m) =>
            {
                // 1.친구 목록에 전달받은 Email의 친구 있는지 확인
                FriendModel? target = _friends.FirstOrDefault(f => f.Email == m.friend.Email);
                if (target == null) return;
                // 있으면 상태 동기화하고 정렬
                target.IsFavorite = m.friend.IsFavorite;
                FriendsList.Refresh();
            });
        }
        /// <summary>
        /// 친구 목록에 정렬과 필터링을 설정합니다.
        /// </summary>
        private void InitializeFriendsListSetting()
        {
            // 1. 필터 조건 설정
            FriendsList.Filter = friendObj =>
            {
                if (friendObj is not FriendModel friend) return false;
                if (string.IsNullOrWhiteSpace(SearchNickname)) return true;

                return friend.Nickname.Contains(SearchNickname, StringComparison.OrdinalIgnoreCase);
            };
            // 2. 정렬 조건 설정
            FriendsList.SortDescriptions.Clear(); // 중복 추가 방지
            // 우선순위 1: 즐겨찾기 기준 (최신순 - 내림차순)
            FriendsList.SortDescriptions.Add(new SortDescription("IsFavorite", ListSortDirection.Descending));
            // 우선순위 2: 즐겨찾기 내에선 닉네임 기준 (오름차순)
            FriendsList.SortDescriptions.Add(new SortDescription("Nickname", ListSortDirection.Ascending));
        }
        #endregion
    }
}
