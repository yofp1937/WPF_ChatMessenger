using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Client.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        public LoginViewModel() { }

        /// <summary>s
        /// Register here를 누르면 회원가입 화면으로 전환되게 Messenger를 통해 MainWindowViewModel에 요청
        /// </summary>
        [RelayCommand] // 
        private void MoveToRegister()
        {
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(RegisterViewModel)));
        }
    }
}
