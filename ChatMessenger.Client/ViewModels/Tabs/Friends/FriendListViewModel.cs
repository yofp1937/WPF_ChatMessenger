using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.DTOs.Responses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;

namespace ChatMessenger.Client.ViewModels.Tabs.Friends
{
    public partial class FriendListViewModel : BaseViewModel
    {
        private readonly IIdentityService _identityService;
        private readonly IFriendService _friendService;

        // 로그인한 사용자의 프로필
        [ObservableProperty]
        private FriendResponse _userProfile;
        // 로그인한 사용자가 추가한 친구 목록
        private List<FriendResponse> _friends = new();
        // View에 Binding되어 보여질 친구 목록(검색, 정렬 값이 바뀔때마다 전환)
        [ObservableProperty]
        private ObservableCollection<FriendResponse> _friendsList = new();

        // 친구 검색과 Binding된 string
        [ObservableProperty]
        private string? _searchNickname = string.Empty;
        // 친구 검색에서 0.35초간 데이터 입력이 없어야 검색 시도하게 만들어주는 토큰
        private CancellationTokenSource? _searchCts;
        // 오른쪽 Panel에 정보를 표시할 선택된 친구 Data
        [ObservableProperty]
        private FriendResponse? _selectedFriend = null;


        public FriendListViewModel(IIdentityService identityService, IFriendService friendService)
        {
            SubscribeMessages();
            _identityService = identityService;
            _friendService = friendService;

            // View에서 Binding하여 사용하기위해 로그인한 User의 FriendResponse 객체 생성
            _userProfile = new FriendResponse()
            {
                Email = _identityService.CurrentUserEmail!,
                Nickname = _identityService.Nickname!,
                StatusMessage = _identityService.StatusMessage!,
                ProfileImageURL = _identityService.ProfileImageURL!,
                IsMe = true,
            };

            // 친구 목록 불러오기
            LoadFriendsCommand.Execute(null);
        }
        #region RelayCommand
        /// <summary>
        /// 서버로부터 친구 목록을 불러와 Friends Property에 채웁니다.
        /// </summary>
        [RelayCommand] // 나중에 친구 목록 새로고침 기능이 추가될수있으니 RelayCommand로 선언해둠
        private async Task LoadFriendsAsync()
        {
            List<FriendResponse>? result = await _friendService.GetFriendsListAsync();
            if (result == null) return;

            // DeferRefresh를 사용하여 using 블록 내에서는 필터 적용을 하지않고 종료할때 한번만 필터링함
            _friends = result;
            ApplyFilterAndSort();
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
        /// <param name="value"></param>
        partial void OnSearchNicknameChanged(string? value)
        {
            // 기존 요청 취소
            _searchCts?.Cancel();
            _searchCts = new();
            CancellationToken token = _searchCts.Token;

            // 비동기로 0.35초 대기 후 실행 명령
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(350, token);
                    if (token.IsCancellationRequested) return;
                    // 0.35초 지났으면 UI 스레드에서 필터 적용
                    App.Current.Dispatcher.Invoke(() => ApplyFilterAndSort());
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
        partial void OnSelectedFriendChanged(FriendResponse? value)
        {
            if(value == null) return;

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
                ApplyFilterAndSort();
            });
            WeakReferenceMessenger.Default.Register<FriendDeletedMessage>(this, (r, m) =>
            {
                // 1.친구 목록에 전달받은 Email의 친구 있는지 확인
                FriendResponse? target = _friends.FirstOrDefault(f => f.Email == m.friend.Email);
                if(target == null) return;
                // 2.있으면 삭제하고 정렬
                _friends.Remove(target);
                ApplyFilterAndSort();
            });
            WeakReferenceMessenger.Default.Register<FriendStatusChangeMessage>(this, (r, m) =>
            {
                // 1.친구 목록에 전달받은 Email의 친구 있는지 확인
                FriendResponse? target = _friends.FirstOrDefault(f => f.Email == m.friend.Email);
                if (target == null) return;
                // 있으면 상태 동기화하고 정렬
                target.IsFavorite = m.friend.IsFavorite;
                ApplyFilterAndSort();
            });
        }
        /// <summary>
        /// 사용자가 직접 호출하는 필터링/정렬 메서드
        /// </summary>
        private void ApplyFilterAndSort()
        {
            // LINQ를 사용하여 필터링과 정렬을 한 번에 처리
            // 1.즐겨찾기 우선
            // 2.닉네임 순
            List<FriendResponse> filtered = _friends.Where(f => string.IsNullOrWhiteSpace(SearchNickname) ||
                                                                          f.Nickname.Contains(SearchNickname, StringComparison.OrdinalIgnoreCase))
                                                                 .OrderByDescending(f => f.IsFavorite)
                                                                 .ThenBy(f => f.Nickname)
                                                                 .ToList();
            // UI 갱신 (새로운 컬렉션으로 교체)
            // Todo: 정렬할때마다 새로운 FriendList 생성 말고 더 좋은 방법 있는지 나중에 찾아보기
            FriendsList = new ObservableCollection<FriendResponse>(filtered);
        }
        #endregion
    }
}
