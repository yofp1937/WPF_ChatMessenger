using ChatMessenger.Client.Common.Enums;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Client.ViewModels.Tabs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Pages
{
    public partial class MainShellViewModel : PageViewModelBase
    {
        // 하위 TabViewModel들은 생성하여 갖고있다가 CurrentViewModel이 바뀌면 할당해줌
        [ObservableProperty]
        private ListPanelViewModel _listPanelVM;
        [ObservableProperty]
        private ContentPanelViewModel _contentPanelVM;

        public MainShellViewModel(IServiceProvider serviceProvider)
        {
            // ListPanel, ContentPanel 주입
            _listPanelVM = serviceProvider.GetRequiredService<ListPanelViewModel>();
            _contentPanelVM = serviceProvider.GetRequiredService<ContentPanelViewModel>();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 하위 ViewModel들의 CleanUp도 호출합니다.
        /// </remarks>
        public override void CleanUp()
        {
            base.CleanUp();
            ListPanelVM.CleanUp();
            ContentPanelVM.CleanUp();
        }

        /// <summary>
        /// Logout 요청을 보냅니다.
        /// </summary>
        [RelayCommand]
        private async Task Logout()
        {
            WeakReferenceMessenger.Default.Send(new ForceLogoutMessage());
        }

        [RelayCommand]
        private void Navigate(ListPanelType type)
        {
            ListPanelVM.ChangeTab(type);
        }
    }
}