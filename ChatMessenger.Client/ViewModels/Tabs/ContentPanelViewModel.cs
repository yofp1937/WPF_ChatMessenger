using ChatMessenger.Client.Common.Enums;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.Common.Messages.Tab;
using ChatMessenger.Client.Common.Messages.Tab.Chat;
using ChatMessenger.Client.Common.Messages.Tab.Friend;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Client.ViewModels.Tabs.Chats;
using ChatMessenger.Client.ViewModels.Tabs.Friends;
using ChatMessenger.Client.ViewModels.Tabs.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client.ViewModels.Tabs
{
    /// <summary>
    /// MainShellView에서 오른쪽 ContentPanel을 담당하는 ViewModel입니다.
    /// </summary>
    public partial class ContentPanelViewModel : TabViewModelBase
    {
        private readonly IFriendService _friendService;
        private FriendDetailViewModel _friendDetailVM;
        private ChatRoomViewModel _chatRoomVM;
        private SettingDetailViewModel _settingDetailVM;
        private CreateChatRoomViewModel _createChatRoomVM;

        [ObservableProperty]
        private BaseViewModel? _currentVM;

        // 친구 추가 모드 여부
        [ObservableProperty]
        private bool _isAddFriendMode = false;
        // 친구 추가 검색 Text
        [ObservableProperty]
        private string? _searchEmail = string.Empty;
        [ObservableProperty]
        private string? _addFriendWarningText = string.Empty;

        public ContentPanelViewModel(IServiceProvider serviceProvider, IFriendService friendService)
        {
            _friendService = friendService;
            _friendDetailVM = serviceProvider.GetRequiredService<FriendDetailViewModel>();
            _chatRoomVM = serviceProvider.GetRequiredService<ChatRoomViewModel>();
            _settingDetailVM = serviceProvider.GetRequiredService<SettingDetailViewModel>();
            _createChatRoomVM = serviceProvider.GetRequiredService<CreateChatRoomViewModel>();

            Subscribe();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// 하위 ViewModel들의 CleanUp도 호출합니다.
        /// </remarks>
        public override void CleanUp()
        {
            base.CleanUp();
            _friendDetailVM.CleanUp();
            _chatRoomVM.CleanUp();
            _settingDetailVM.CleanUp();
            _createChatRoomVM.CleanUp();
        }

        #region RelayCommand
        /// <summary>
        /// 친구 검색 버튼을 눌렀을때 동작하는 Command
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        private async Task SearchFriendAsync()
        {
            AddFriendWarningText = string.Empty;
            if (string.IsNullOrEmpty(SearchEmail))
            {
                AddFriendWarningText = "이메일을 입력해주세요.";
                return;
            }
            try
            {
                // 1. 서버에 검색 요청
                FriendModel? result = await _friendService.SearchFriendAsync(SearchEmail);
                if (result == null) return;

                // 2. 검색 성공 시 상세 정보 업데이트
                CurrentVM = _friendDetailVM;
                _friendDetailVM.SetFriendProfile(result);
                SearchEmail = string.Empty;
                // 3.FriendList의 ListBox 선택 상태 제거
                WeakReferenceMessenger.Default.Send(new SelectedFriendResetMessage());
            }
            catch (Exception ex)
            {
                AddFriendWarningText = ex.Message;
            }
        }

        /// <summary>
        /// 친구 검색 Panel 닫기 Command
        /// </summary>
        [RelayCommand]
        private void CloseAddFriendMode()
        {
            IsAddFriendMode = false;
        }
        #endregion RelayCommand

        #region OnChanged
        /// <summary>
        /// IsAddFriendMode가 false로 변하면 검색창에 입력된 데이터를 초기화합니다.
        /// </summary>
        partial void OnIsAddFriendModeChanged(bool value)
        {
            if (!value)
            {
                SearchEmail = string.Empty;
                AddFriendWarningText = string.Empty;
            }
        }
        #endregion OnChanged

        #region private Method
        /// <summary>
        /// 메세지나 이벤트를 구독합니다.
        /// </summary>
        private void Subscribe()
        {
            // ConetentPanel의 화면을 변경해야할때 호출되는 메세지를 구독합니다.
            WeakReferenceMessenger.Default.Register<ChangeContentMessage>(this, (r, m) =>
            {
                CurrentVM = m.type switch
                {
                    ContentPanelType.Friend => _friendDetailVM,
                    ContentPanelType.Chat => _chatRoomVM,
                    ContentPanelType.Setting => _settingDetailVM,
                    _ => _friendDetailVM
                };
            });
            // ContentPanel 화면을 메세지에 포함된 User의 Profile 화면으로 교체합니다. 
            WeakReferenceMessenger.Default.Register<FriendSelectionChangedMessage>(this, (r, m) =>
            {
                CurrentVM = _friendDetailVM;
                _friendDetailVM.SetFriendProfile(m.friend);
            });
            // AddFriendMode 신호가 오면 반대 값으로 변경합니다.
            WeakReferenceMessenger.Default.Register<AddFriendModeChangedMessage>(this, (r, m) =>
            {
                IsAddFriendMode = !IsAddFriendMode;
            });
            // ContentPanel 화면을 메세지에 포함된 roomId 채팅방 화면으로 교체합니다.
            WeakReferenceMessenger.Default.Register<ChatRoomSelectionChangedMessage>(this, (r, m) =>
            {
                CurrentVM = _chatRoomVM;
                _chatRoomVM.SetChatRoom(m.roomId);
            });
            // ContentPanel 화면을 CreateChatRoomView로 변경합니다.
            WeakReferenceMessenger.Default.Register<OpenCreateChatRoomRequestMessage>(this, (r, m) =>
            {
                if (CurrentVM == _createChatRoomVM) return;
                CurrentVM = _createChatRoomVM;
                _ = _createChatRoomVM.ResetInputValues();
            });
        }
        #endregion private Method
    }
}
