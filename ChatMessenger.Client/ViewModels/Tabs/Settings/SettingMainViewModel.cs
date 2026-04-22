/*
 * MainShellView에서 설정 탭의 전반적인 데이터 처리를 담당하는 ViewModel입니다.
 */
using ChatMessenger.Client.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Tabs.Settings
{
    public partial class SettingMainViewModel : TabViewModelBase
    {
        public SettingMainViewModel(IServiceProvider serviceProvider)
        {
            LeftContentViewModel = serviceProvider.GetRequiredService<SettingListViewModel>();
            RightContentViewModel = serviceProvider.GetRequiredService<SettingDetailViewModel>();
        }
    }
}
