using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.Common.Messages.Tab.Friend;
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
        [ObservableProperty]
        private string? _editProfileWarningText = string.Empty;

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
        }

        #region public Method
        /// <summary>
        /// View에 친구의 Profile을 표시합니다
        /// </summary>
        /// <param name="friend">View에 표시할 User의 정보가 들어있는 FriendModel</param>
        public void SetFriendProfile(FriendModel friend)
        {
            CurrentFriend = friend;
            IsEditMode = false;
        }
        #endregion
        #region RelayCommand
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
        /// Nickname 입력 TextBox에 사용자가 값을 입력하면 상황에따라 경고 메세지를 띄워줍니다.
        /// </summary>
        /// <param name="value">사용자가 입력한 문자열</param>
        partial void OnEditNicknameChanged(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                EditProfileWarningText = "닉네임을 입력해주세요.";
                return;
            }

            if (value.Length < 4)
            {
                EditProfileWarningText = "닉네임은 최소 4글자 이상이어야 합니다.";
            }
            else
            {
                EditProfileWarningText = string.Empty;
            }
        }
        #endregion
    }
}
