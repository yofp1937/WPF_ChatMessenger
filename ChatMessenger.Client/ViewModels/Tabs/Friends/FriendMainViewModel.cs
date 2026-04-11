/*
 * MainShellView에서 왼쪽, 오른쪽 화면 ViewModel을 통합 관리하는 ViewModel입니다.
 */
using ChatMessenger.Client.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Tabs.Friends
{
    public partial class FriendMainViewModel : TabViewModelBase
    {
        public FriendMainViewModel(IServiceProvider serviceProvider)
        {
            LeftContentViewModel = serviceProvider.GetRequiredService<FriendListViewModel>();
            RightContentViewModel = serviceProvider.GetRequiredService<FriendDetailViewModel>();
        }
    }
}
