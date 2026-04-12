using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.DTOs.Responses;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ChatMessenger.Client.ViewModels.Tabs.Friends
{
    public partial class FriendListViewModel : BaseViewModel
    {
        private readonly IIdentityService _identityService;
        private readonly IFriendService _friendService;

        // 로그인한 사용자의 프로필
        [ObservableProperty]
        private FriendResponse _userProfile;
        // UI에 Binding될 친구 목록 List
        [ObservableProperty]
        private ObservableCollection<FriendResponse> _friends = new();
        // 왼쪽 Panel에 정보를 표시할 선택된 Data
        [ObservableProperty]
        private object? _selectedFriend;

        public FriendListViewModel(IIdentityService identityService, IFriendService friendService)
        {
            _identityService = identityService;
            _friendService = friendService;

            // View에서 Binding하여 사용하기위해 로그인한 User의 FriendResponse 객체 생성
            _userProfile = new FriendResponse()
            {
                Email = _identityService.CurrentUserEmail!,
                Nickname = _identityService.Nickname!,
                StatusMessage = _identityService.StatusMessage!,
                ProfileImageURL = _identityService.ProfileImageURL!,
            };

            // ViewModel 생성시 친구 목록 불러오기
            LoadFriendsCommand.Execute(null);
        }

        /// <summary>
        /// 서버로부터 친구 목록을 불러와 Friends Property에 채웁니다.
        /// </summary>
        /// <returns></returns>
        [RelayCommand] // 나중에 친구 목록 새로고침 기능이 추가될수있으니 RelayCommand로 선언해둠
        private async Task LoadFriendsAsync()
        {
            List<FriendResponse>? result = await _friendService.GetFriendsListAsync();
            if (result == null) return;

            Friends.Clear();
            foreach (FriendResponse friend in result)
            {
                Friends.Add(friend);
            }
        }
    }
}
