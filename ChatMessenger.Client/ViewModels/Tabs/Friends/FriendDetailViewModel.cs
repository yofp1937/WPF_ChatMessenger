using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
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
        // 친구 추가 검색 Text
        [ObservableProperty]
        private string? _searchEmail = string.Empty;
        [ObservableProperty]
        private string? _warningText = string.Empty;

        // 프로필 표시용
        [ObservableProperty]
        private FriendModel? _currentFriend;

        // 내 프로필 수정 모드
        [ObservableProperty]
        private bool _isEditMode = false;
        // 수정 모드에서 임시로 담아둘 값들
        [ObservableProperty]
        private string? _editNickname;
        [ObservableProperty]
        private string? _editStatusMessage;

        public FriendDetailViewModel(IIdentityService identityService, IFriendService friendService)
        {
            _friendService = friendService;

            // 로그인에 성공했지만 Profile 정보가 존재하지않으면 강제 로그아웃
            if (identityService.MyProfile == null)
            {
                WeakReferenceMessenger.Default.Send(new ForceLogoutMessage());
                return;
            }
            CurrentFriend = identityService.MyProfile;

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
                FriendModel? result = await _friendService.SearchFriendAsync(SearchEmail);
                if (result == null) return;

                // 2. 검색 성공 시 상세 정보 업데이트
                OnFriendReceived(result);
                SearchEmail = string.Empty;
                // 3.FriendList의 ListBox 선택 상태 제거
                WeakReferenceMessenger.Default.Send(new SelectedFriendResetMessage());
            }
            catch (Exception ex)
            {
                WarningText = ex.Message;
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
            if (!IsEditMode)
            {
                IsEditMode = true;
                EditNickname = CurrentFriend?.Nickname;
                EditStatusMessage = CurrentFriend?.StatusMessage;
            }
            else
            {
                IsEditMode = false;
            }
        }

        /// <summary>
        /// 친구 추가 버튼을 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task AddFriendAsync()
        {
            if (CurrentFriend == null) return;
            try
            {
                // 1. 현재 화면에 표시되는 유저를 친구 추가 요청
                FriendModel? result = await _friendService.AddFriendAsync(CurrentFriend.Email);
                if (result == null) return;

                // 2. 친구 추가 완료되면 CurrentFriend 갱신하여 화면 갱신
                CurrentFriend = result;
                // 3. FriendList도 갱신하라고 알림
                WeakReferenceMessenger.Default.Send(new FriendAddedMessage(result));
            }
            catch
            {
                Debug.WriteLine($"[{GetType().Name}_AddFriendAsync]: 친구 추가 실패");
            }
        }

        /// <summary>
        /// 친구 삭제 버튼 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task DeleteFriendAsync()
        {
            if (CurrentFriend == null) return;
            try
            {
                // 1. 현재 화면에 표시되는 유저를 친구 삭제 요청
                bool result = await _friendService.DeleteFriendAsync(CurrentFriend.Email);
                if (!result) return;

                // 2. 친구 삭제 성공하면 CurrentFriend 
                CurrentFriend.IsAdded = false;
                CurrentFriend.IsFavorite = false;
                // 3. FriendList도 갱신하라고 알림
                WeakReferenceMessenger.Default.Send(new FriendDeletedMessage(CurrentFriend));
            }
            catch
            {
                Debug.WriteLine($"[{GetType().Name}_DeleteFriendAsync]: 친구 삭제 실패");
            }
        }

        /// <summary>
        /// 즐겨찾기 등록, 해제 버튼 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task UpdateFavorite()
        {
            if (CurrentFriend == null) return;
            try
            {
                // 업데이트하려는 값은 항상 현재 상태의 반대값
                bool targetStatus = !CurrentFriend.IsFavorite;
                bool result = await _friendService.UpdateFavoriteAsync(CurrentFriend.Email, targetStatus);
                if (!result) return;

                // 변경 성공시 로컬 데이터 갱신
                CurrentFriend.IsFavorite = targetStatus;
                // 친구 목록 즐겨찾기 아이콘 갱신
                WeakReferenceMessenger.Default.Send(new FriendStatusChangeMessage(CurrentFriend));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{GetType().Name}_UpdateFavoriteAsync]: {ex.Message}");
            }
        }

        /// <summary>
        /// 차단, 차단 해제 버튼 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task UpdateBlocked()
        {
            if (CurrentFriend == null) return;
            try
            {
                // 업데이트하려는 값은 항상 현재 상태의 반대값
                bool targetStatus = !CurrentFriend.IsBlocked;
                bool result = await _friendService.UpdateBlockAsync(CurrentFriend.Email, targetStatus);
                // 변경 실패시 return
                if (!result) return;

                // 변경 성공시 로컬 데이터 갱신
                CurrentFriend.IsBlocked = targetStatus;
                // 1.차단 성공시
                if (targetStatus)
                {
                    // 서버 로직과 동일하게 차단했을시 즐겨찾기 해제
                    CurrentFriend.IsAdded = false;
                    CurrentFriend.IsFavorite = false;
                    // 추가돼있던 친구일수있으니 FriendList에서 제거 요청
                    WeakReferenceMessenger.Default.Send(new FriendDeletedMessage(CurrentFriend));
                }
                // 2.차단 해제시
                else
                {
                    // 상태 초기화
                    CurrentFriend.IsAdded = false;
                    CurrentFriend.IsFavorite = false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{GetType().Name}_UpdateBlockAsync]: {ex.Message}");
            }
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
                OnFriendReceived(m.friend);
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
        private void OnFriendReceived(FriendModel friend)
        {
            CurrentFriend = friend;
            IsEditMode = false;
        }
        #endregion
    }
}
