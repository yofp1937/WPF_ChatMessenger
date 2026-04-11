using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Client.ViewModels.Tabs.Friends;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Pages
{
    public partial class MainShellViewModel : PageViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IIdentityService _identityService;

        [ObservableProperty]
        private TabViewModelBase _currentViewModel;

        public MainShellViewModel(IServiceProvider serviceProvider, IIdentityService identityService)
        {
            _serviceProvider = serviceProvider;
            _identityService = identityService;

            _currentViewModel = _serviceProvider.GetRequiredService<FriendMainViewModel>();
        }

        /// <summary>
        /// Logout을 누르면 _identityService의 내부 값을 초기화하고 로그인 화면으로 이동합니다.
        /// </summary>
        [RelayCommand]
        private void Logout()
        {
            _identityService.Logout();
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(LoginViewModel)));
        }
    }
}