/* 
 * MainWindow에서 사용자의 입력에따라 CurrentState가 변경되고,
 * CurrentState에 따라 화면의 View를 갈아끼워줌
 * 
 * 기능
 * 1.기존 LoginView에서 RegisterView, MainShellView로 이동 가능
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Client.ViewModels.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Windows
{
    /* 
     * CommunityToolkit에서 ObservableProperty를 사용하면
     * Toolkit이 컴파일 과정에서 내가 선언한 Field(private)들을 Property(public)로 생성해주는데
     * partial은 하나의 클래스를 여러개의 파일로 작성한다는 의미로
     * 내가 작성한 partial MainViewModel과 Toolkit이 작성한 partial MainViewModel을 합쳐서 실행할 수 있게 만들어줌
     */
    public partial class MainWindowViewModel : WindowViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        [ObservableProperty] // 아래에 선언한 Field의 Property를 자동 생성하고 Field의 값이 바뀌면 자동으로 OnPropertyChanged도 호출해줌
        private PageViewModelBase _currentViewModel; // 현재 화면에따라 ViewModel을 생성하여 담는 변수

        public MainWindowViewModel(IWindowControlService windowControlService , IServiceProvider serviceProvider) : base(windowControlService)
        {
            _serviceProvider = serviceProvider;
            CurrentViewModel = _serviceProvider.GetRequiredService<LoginViewModel>();
            SubscribeMessages();
        }

        private void SubscribeMessages()
        {
            WeakReferenceMessenger.Default.Register<NavigationMessage>(this, (r, m) =>
            {
                // 받은 메시지에 담긴 타입으로 서비스를 가져와서 화면을 교체합니다.
                CurrentViewModel = (PageViewModelBase)_serviceProvider.GetRequiredService(m.ViewModelType);
            });
        }
    }
}
