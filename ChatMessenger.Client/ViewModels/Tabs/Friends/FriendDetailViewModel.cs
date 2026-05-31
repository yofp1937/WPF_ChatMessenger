using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.Common.Messages.Tab.Friend;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.Common;
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
                EditNickname = CurrentFriend?.Nickname;
                EditStatusMessage = CurrentFriend?.StatusMessage;
            }
            IsEditMode = !IsEditMode;
        }
        /// <summary>
        /// 친구 추가 버튼을 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task AddFriendAsync()
        {
            if (CurrentFriend == null)
                return;

            // 1. 현재 화면에 표시되는 유저를 친구 추가 요청
            ServiceResult<FriendModel> response = await _friendService.AddFriendAsync(CurrentFriend.Email);
            if (!response.IsSuccess)
                return;

            // 2. 친구 추가 완료되면 CurrentFriend 갱신하여 화면 갱신
            CurrentFriend = response.Data;
            // 3. FriendList도 갱신하라고 알림
            WeakReferenceMessenger.Default.Send(new FriendAddedMessage(response.Data));
        }
        /// <summary>
        /// 친구 삭제 버튼 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task DeleteFriendAsync()
        {
            if (CurrentFriend == null) return;
            // 1. 현재 화면에 표시되는 유저를 친구 삭제 요청
            ServiceResult<bool> response = await _friendService.DeleteFriendAsync(CurrentFriend.Email);
            if (!response.IsSuccess)
                return;

            // 2. 친구 삭제 성공하면 CurrentFriend 
            CurrentFriend.IsAdded = false;
            CurrentFriend.IsFavorite = false;
            // 3. FriendList도 갱신하라고 알림
            WeakReferenceMessenger.Default.Send(new FriendDeletedMessage(CurrentFriend));
        }
        /// <summary>
        /// 즐겨찾기 등록, 해제 버튼 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task UpdateFavorite()
        {
            if (CurrentFriend == null) return;
            Debug.WriteLine($"현재 favorite: {CurrentFriend.IsFavorite}, 변경 요청 값: {!CurrentFriend.IsFavorite}");

            ServiceResult<bool> response = await _friendService.UpdateFavoriteAsync(CurrentFriend.Email, !CurrentFriend.IsFavorite);
            if (!response.IsSuccess)
                return;

            // 변경 성공시 로컬 데이터 갱신
            CurrentFriend.IsFavorite = !CurrentFriend.IsFavorite;
            // 친구 목록 즐겨찾기 아이콘 갱신
            WeakReferenceMessenger.Default.Send(new FriendStatusChangeMessage(CurrentFriend));
        }
        /// <summary>
        /// 차단, 차단 해제 버튼 눌렀을때 동작하는 Command
        /// </summary>
        [RelayCommand]
        private async Task UpdateBlocked()
        {
            if (CurrentFriend == null) return;
            Debug.WriteLine($"현재 Blocked: {CurrentFriend.IsBlocked}, 변경 요청 값: {!CurrentFriend.IsBlocked}");

            // 1. 차단 상태 변경 요청
            bool nextState = !CurrentFriend.IsBlocked;
            ServiceResult<bool> response = await _friendService.UpdateBlockAsync(CurrentFriend.Email, nextState);
            if (!response.IsSuccess)
                return;

            // 2. 차단, 차단해제 성공시 메모리 값 수정
            CurrentFriend.IsBlocked = nextState;
            CurrentFriend.IsAdded = false;
            CurrentFriend.IsFavorite = false;
            // 3. 차단한거면 친구 목록에서 삭제 요청
            if (nextState)
                WeakReferenceMessenger.Default.Send(new FriendDeletedMessage(CurrentFriend));
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
