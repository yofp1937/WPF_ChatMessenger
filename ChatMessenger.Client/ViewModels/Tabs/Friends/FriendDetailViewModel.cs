using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.DTOs.Responses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;

namespace ChatMessenger.Client.ViewModels.Tabs.Friends
{
    public partial class FriendDetailViewModel : BaseViewModel
    {
        private readonly IFriendService _friendService;

        // 친구 추가 모드 여부
        [ObservableProperty]
        private bool _isAddFriendMode = false;
        // 내 프로필 수정 모드
        [ObservableProperty]
        private bool _isEditMode = false;
        // 친구 추가 검색 Text
        [ObservableProperty]
        private string? _searchEmail = string.Empty;
        [ObservableProperty]
        private string? _warningText = string.Empty;

        // 프로필 표시용
        [ObservableProperty]
        private FriendResponse? _currentFriend;

        public FriendDetailViewModel(IIdentityService identityService, IFriendService friendService)
        {
            _friendService = friendService;

            // View에서 Binding하여 사용하기위해 로그인한 User의 FriendResponse 객체 생성
            CurrentFriend = new FriendResponse()
            {
                Email = identityService.CurrentUserEmail!,
                Nickname = identityService.Nickname!,
                StatusMessage = identityService.StatusMessage!,
                ProfileImageURL = identityService.ProfileImageURL!,
                IsMe = true,
            };

            SubscribeMessages();
        }

        #region RelayCommand
        /// <summary>
        /// 친구 검색 버튼을 눌렀을때 동작하는 Command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task SearchFriendAsync()
        {
            WarningText = string.Empty;
            if (string.IsNullOrEmpty(SearchEmail))
            {
                WarningText = "이메일을 입력해주세요.";
                return;
            }
            try
            {
                // 1. 서버에 검색 요청
                FriendResponse? result = await _friendService.SearchFriendAsync(SearchEmail);
                if (result == null) return;

                // 2. 검색 성공 시 상세 정보 업데이트
                OnFriendReceived(result);
                SearchEmail = string.Empty;
                WeakReferenceMessenger.Default.Send(new SelectedFriendResetMessage());
            }
            catch (Exception ex)
            {
                WarningText = ex.Message;
                Debug.WriteLine($"[{this.GetType().Name}_{nameof(SearchFriendAsync)}] - Error: {ex.Message}");
            }
        }
        /// <summary>
        /// 친구 검색 Panel 닫기 Command
        /// </summary>
        [RelayCommand]
        private void CloseAddFriendMode()
        {
            IsAddFriendMode = false;
        }
        /// <summary>
        /// 내 프로필 수정 모드 변경 Command
        /// </summary>
        [RelayCommand]
        private void ChangeProfileEditMode()
        {
            IsEditMode = !IsEditMode;
        }
        #endregion
        #region OnChanged Method
        /// <summary>
        /// IsAddFriendMode가 false로 변하면 검색창에 입력된 데이터를 초기화합니다.
        /// </summary>
        partial void OnIsAddFriendModeChanged(bool value)
        {
            if (!value)
            {
                SearchEmail = string.Empty;
                WarningText = string.Empty;
            }
        }
        #endregion
        #region private Method
        /// <summary>
        /// 메세지를 구독합니다.
        /// </summary>
        private void SubscribeMessages()
        {
            WeakReferenceMessenger.Default.Register<FriendSelectionChangedMessage>(this, (r, m) =>
            {
                OnFriendReceived(m.Value);
            });
            WeakReferenceMessenger.Default.Register<AddFriendModeChangedMessage>(this, (r, m) =>
            {
                // IsAddFriendMode를 반전시킴
                IsAddFriendMode = !IsAddFriendMode;
            });
        }
        /// <summary>
        /// 메신저를통해 데이터를 전달받으면 필드에 데이터를 할당합니다.
        /// </summary>
        private void OnFriendReceived(FriendResponse friend)
        {
            CurrentFriend = friend;
            IsEditMode = false;
        }
        #endregion
    }
}
