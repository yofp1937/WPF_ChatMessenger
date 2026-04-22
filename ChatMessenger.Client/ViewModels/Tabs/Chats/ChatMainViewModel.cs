/*
 * MainShellView에서 채팅 탭의 전반적인 데이터 처리를 담당하는 ViewModel입니다.
 */
using ChatMessenger.Client.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Tabs.Chats
{
    public partial class ChatMainViewModel : TabViewModelBase
    {
        public ChatMainViewModel(IServiceProvider serviceProvider)
        {
            LeftContentViewModel = serviceProvider.GetRequiredService<ChatListViewModel>();
            RightContentViewModel = serviceProvider.GetRequiredService<ChatRoomViewModel>();
        }
    }
}
