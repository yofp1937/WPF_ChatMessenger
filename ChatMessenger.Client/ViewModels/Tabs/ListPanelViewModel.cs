using ChatMessenger.Client.Common.Enums;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Client.ViewModels.Tabs.Chats;
using ChatMessenger.Client.ViewModels.Tabs.Friends;
using ChatMessenger.Client.ViewModels.Tabs.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Tabs
{
    /// <summary>
    /// MainShellView에서 왼쪽 ListPanel을 담당하는 ViewModel입니다.
    /// </summary>
    public partial class ListPanelViewModel : TabViewModelBase
    {
        // 상시 갖고있어야하는 ListVM
        private FriendListViewModel _friendList;
        private ChatListViewModel _chatList;
        private SettingListViewModel _settingList;

        [ObservableProperty]
        private BaseViewModel _currentVM;

        public ListPanelViewModel(IServiceProvider serviceProvider)
        {
            _friendList = serviceProvider.GetRequiredService<FriendListViewModel>();
            _chatList = serviceProvider.GetRequiredService<ChatListViewModel>();
            _settingList = serviceProvider.GetRequiredService<SettingListViewModel>();

            CurrentVM = _friendList;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 하위 ViewModel들의 CleanUp도 호출합니다.
        /// </remarks>
        public override void CleanUp()
        {
            base.CleanUp();
            _friendList.CleanUp();
            _chatList.CleanUp();
            _settingList.CleanUp();
        }

        /// <summary>
        /// 현재 화면을 변경합니다.
        /// </summary>
        /// <param name="type">변경할 화면 타입</param>
        public void ChangeTab(ListPanelType type)
        {
            CurrentVM = type switch
            {
                ListPanelType.Friend => _friendList,
                ListPanelType.Chat => _chatList,
                ListPanelType.Setting => _settingList,
                _ => _friendList
            };
        }
    }
}
