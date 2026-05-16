using ChatMessenger.Client.Common.Enums;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages.Page;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.DTOs.Responses.Auth;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Windows.Controls;

namespace ChatMessenger.Client.ViewModels.Pages
{
    public partial class LoginViewModel : PageViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IIdentityService _identityService;
        private readonly IChatHubService _chatHubService;

        // 사용자 입력 Email
        [ObservableProperty]
        private string? _email;
        // 경고 메세지
        [ObservableProperty]
        private string? _warningText;
        // 로딩 중임을 표시하기 위한 속성
        [ObservableProperty]
        private bool _isBusy;

        public LoginViewModel(IAuthService authService, IIdentityService identityService, IChatHubService chatHubService)
        {
            _authService = authService;
            _identityService = identityService;
            _chatHubService = chatHubService;

            Email = "test1@naver.com";
        }

        /// <summary>
        /// Register here를 누르면 회원가입 화면으로 전환되게 Messenger를 통해 MainWindowViewModel에 요청
        /// </summary>
        [RelayCommand]
        private void MoveToRegister()
        {
            WeakReferenceMessenger.Default.Send(new ChangePageMessage(AppPageType.Register));
        }
        /// <summary>
        /// 입력된 Data를 기반으로 Server에 인증을 요청합니다.
        /// <br/>RelayCommand에서 Async는 제거하고 Command를 만들기 때문에
        /// View에서는 SignInCommand에 Binding하여 사용하면 됨
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task SignInAsync(object? parameter)
        {
            if (IsBusy) return;
            // 1. 전달받은 parameter에서 비밀번호 추출
            string? pwd = (parameter as PasswordBox)?.Password;
            // 2. 아이디나 비밀번호가 입력되지 않았으면 return
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(pwd))
            {
                WarningText = "아이디와 비밀번호를 정확하게 입력해주세요.";
                return;
            }

            try
            {
                // TODO : EmailTextBox와 PasswordBox에 접근 불가능하게 변경
                IsBusy = true;
                WarningText = string.Empty;

                // 3. _authService를 이용해 서버에 인증 요청
                LoginResponse? response = await _authService.SignInAsync(Email, pwd);

                if (response != null && response.IsSuccess)
                {
                    // 3-1. 로그인 성공하면 profile 생성 시도
                    FriendModel? profile = response.UserProfile != null ? new FriendModel(response.UserProfile) : null;
                    // profile 생성 실패하면 return
                    if (profile == null)
                    {
                        WarningText = "사용자 정보를 불러오는데 실패했습니다. 다시 시도해주세요";
                        return;
                    }
                    // 3-2. 확실히 검증이 끝나면 데이터 할당하고 View 이동
                    _identityService.Token = response.Token;
                    _identityService.MyProfile = profile;
                    if (!string.IsNullOrEmpty(response.Token))
                        await _chatHubService.ConnectAsync(response.Token);
                    WeakReferenceMessenger.Default.Send(new ChangePageMessage(AppPageType.MainShell));
                }
                else
                {
                    // 3-2. 로그인 실패
                    WarningText = "이메일 혹은 비밀번호가 일치하지 않습니다.";
                }
            }
            catch (Exception ex)
            {
                WarningText = "서버와 통신 중 오류가 발생했습니다. 잠시 후 시도해주세요.";
                Debug.WriteLine($"[Login Error]: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
