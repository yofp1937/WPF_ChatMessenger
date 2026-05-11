using ChatMessenger.Client.Common.Enums;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Client.ViewModels.Tabs.Chats;
using ChatMessenger.Client.ViewModels.Tabs.Friends;
using ChatMessenger.Client.ViewModels.Tabs.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ChatMessenger.Client.ViewModels.Pages
{
    public partial class MainShellViewModel : PageViewModelBase
    {
        private readonly IIdentityService _identityService;
        private readonly IChatHubService _chatHubService;

        // 하위 TabViewModel들은 생성하여 갖고있다가 CurrentViewModel이 바뀌면 할당해줌
        private readonly FriendMainViewModel _friendViewModel;
        private readonly ChatMainViewModel _chatViewModel;
        private readonly SettingMainViewModel _settingViewModel;

        [ObservableProperty]
        private TabViewModelBase _currentViewModel;

        public MainShellViewModel(IIdentityService identityService, IChatHubService chatHubService,
                                            FriendMainViewModel friendViewModel, ChatMainViewModel chatViewModel,
                                            SettingMainViewModel settingViewModel)
        {
            // service 주입
            _identityService = identityService;
            _chatHubService = chatHubService;

            // 하위 TabViewModel 주입
            _friendViewModel = friendViewModel;
            _chatViewModel = chatViewModel;
            _settingViewModel = settingViewModel;

            // 필드 값 설정
            _currentViewModel = _friendViewModel;
        }

        /// <summary>
        /// Logout을 누르면 _identityService의 내부 값을 초기화하고 로그인 화면으로 이동합니다.
        /// </summary>
        [RelayCommand]
        private async Task Logout()
        {
            await _chatHubService.DisconnectAsync();
            _identityService.Logout();
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(LoginViewModel)));
        }

        [RelayCommand]
        private void Navigate(MainTabType type)
        {
            CurrentViewModel = type switch
            {
                MainTabType.Friends => _friendViewModel,
                MainTabType.Chats => _chatViewModel,
                MainTabType.Settings => _settingViewModel,
                _ => _friendViewModel
            };
        }
    }
}