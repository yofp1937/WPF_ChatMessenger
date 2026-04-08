using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ChatMessenger.Client.ViewModels
{
    public partial class MainShellViewModel : BaseViewModel
    {
        private readonly IIdentityService _identityService;

        public MainShellViewModel(IIdentityService identityService)
        {
            _identityService = identityService;
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
