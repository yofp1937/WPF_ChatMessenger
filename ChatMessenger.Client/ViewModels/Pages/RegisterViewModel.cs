using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Diagnostics;
using System.Windows.Controls;

namespace ChatMessenger.Client.ViewModels.Pages
{
    public partial class RegisterViewModel : PageViewModelBase
    {
        private readonly IAuthService _authService;

        // 사용자 입력 Email
        [ObservableProperty]
        private string? _email;
        // 사용자 입력 Nickname
        [ObservableProperty]
        private string? _nickname;
        // 경고 메세지
        [ObservableProperty]
        private string? _warningText;
        // 로딩 중임을 표시하기 위한 속성
        [ObservableProperty]
        private bool _isBusy;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// 뒤로가기 버튼을 누르면 로그인 화면으로 전환되게 Messenger를 통해 MainWindowViewModel에 요청합니다.
        /// </summary>
        [RelayCommand]
        private void MoveToLogin()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(LoginViewModel)));
        }

        /// <summary>
        /// Create Account 버튼을 클릭하면 Server에 회원가입을 요청합니다.
        /// </summary>
        [RelayCommand]
        private async Task Register(object? parameter)
        {
            if (IsBusy) return;
            // 1.parameter로 받은 비밀번호 해체
            var (pwd, pwdCheck) = ExtractPassword(parameter);

            // 2.유효성 검사
            if (!ValidateInput(pwd, pwdCheck)) return;

            // 3.회원가입 요청
            try
            {
                IsBusy = true;
                WarningText = string.Empty;

                bool isSuccess = await _authService.RegisterAsync(Email!, pwd, Nickname!);
                if (isSuccess)
                {
                    MoveToLogin();
                }
                else
                {
                    WarningText = "이미 회원가입된 이메일입니다.";
                }
            }
            catch (Exception ex)
            {
                WarningText = "서버 통신 중 오류가 발생했습니다.";
                System.Diagnostics.Debug.WriteLine($"[RegisterViewModel - Register Error]: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Parameter(object[])에서 두개의 PasswordBox 값을 추출합니다.
        /// </summary>
        private (string pwd, string pwdCheck) ExtractPassword(object? parameter)
        {
            Debug.WriteLine($"parameter - {parameter}");
            if (parameter is object[] passwordBoxes)
            {
                PasswordBox? pb1 = passwordBoxes[0] as PasswordBox;
                Debug.WriteLine($"pb1 - {pb1}");
                PasswordBox? pb2 = passwordBoxes[1] as PasswordBox;
                Debug.WriteLine($"pb2 - {pb2}");
                return (pb1?.Password ?? string.Empty, pb2?.Password ?? string.Empty);
            }
            return (string.Empty, string.Empty);
        }

        /// <summary>
        /// 입력된 데이터들의 유효성을 검사합니다.
        /// </summary>
        private bool ValidateInput(string pwd, string pwdCheck)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Nickname))
            {
                WarningText = "이메일과 닉네임을 입력해주세요.";
                return false;
            }
            if (string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(pwdCheck))
            {
                WarningText = "비밀번호를 입력해주세요.";
                return false;
            }
            if (pwd != pwdCheck)
            {
                WarningText = "비밀번호가 일치하지 않습니다.";
                return false;
            }
            return true;
        }
    }
}
