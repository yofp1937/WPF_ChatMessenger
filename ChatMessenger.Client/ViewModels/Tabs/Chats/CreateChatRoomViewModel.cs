using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages.Tab;
using ChatMessenger.Client.Common.Messages.Tab.Chat.Room;
using ChatMessenger.Client.Models.Friends;
using ChatMessenger.Client.ViewModels.Base;
using ChatMessenger.Shared.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ChatMessenger.Client.ViewModels.Tabs.Chats
{
    public partial class CreateChatRoomViewModel : BaseViewModel
    {
        private IFriendService _friendService;
        private IChatService _chatService;

        // 그룹 채팅에서 신규 참가자 초대할경우를 대비해 값 저장
        private Guid _targetRoomId = Guid.Empty;
        private string? _profileIMGURL;

        // 방 제목
        [ObservableProperty]
        private string _roomTitle = string.Empty;
        [ObservableProperty]
        // 방 제목 TextBox 접근 활성화 여부
        private bool _isTitleEditEnabled = true;
        [ObservableProperty]
        private string _titleWarningText = string.Empty;

        // 친구 목록
        [ObservableProperty]
        private ObservableCollection<FriendModel> _friendList = new();
        [ObservableProperty]
        private string _inviteWarningText = string.Empty;

        // 인원수 Counting용 Property
        // 초대하려는 총 인원 수
        public int TotalSelectedCount => FriendList.Count(f => f.IsSelected);
        // 새로 채팅방에 추가하기위해 선택한 인원 수
        public int NewInvitedCount => FriendList.Count(f => f.IsSelected && f.IsCheckBoxEnabled);

        public CreateChatRoomViewModel(IFriendService friendService, IChatService chatService)
        {
            _friendService = friendService;
            _chatService = chatService;
        }

        /// <summary>
        /// CreateChatVM 뷰를 생성합니다.
        /// </summary>
        /// <returns></returns>
        public async Task InitCreateChatVMAsync(OpenCreateChatRoomRequestMessage message)
        {
            ResetInputValues();
            await LoadFriendsAsync();

            // 1. 채팅방 기존 참가자 이메일 정보가 존재하면 CheckBox Lock 제어
            if (message.ParticipantEmails != null && message.ParticipantEmails.Count > 0)
                UpdateParticipantStatus(message.ParticipantEmails);

            // 2. 기존 채팅방에 인원 추가인 경우 데이터 세팅
            if (message.RoomId.HasValue)
                SetExsitingRoomData(message);

            // 3. UI 갱신 신호 전송
            ValidateRoomTitle();
            RefreshCountAndCommand();
        }
        #region RelayCommand
        /// <summary>
        /// 초대 버튼이 클릭되면 동작합니다.
        /// </summary>
        /// <remarks>
        /// FriendList에서 IsSelected가 true인 User만 골라서 새로운 채팅방을 생성합니다.
        /// </remarks>
        /// <returns></returns>
        [RelayCommand(CanExecute = nameof(CanCreateChatRoom))]
        private async Task CreateChatRoom()
        {
            // 1. 기존 채팅방에 신규 참가자를 초대하려는 경우
            if (_targetRoomId != Guid.Empty)
                await InviteNewChatParticipant();
            // 2. 신규 채팅방을 개설하는 경우
            else
                await CreateNewChatRoom();
        }
        [RelayCommand]
        private async Task CloseView()
        {
            WeakReferenceMessenger.Default.Send(new ChangeContentMessage(null));
        }
        #endregion RelayCommand
        #region OnChanged Method
        /// <summary>
        /// 사용자가 RoomTitle TextBox를 통해 제목을 입력하면 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 0.35초간 데이터 입력이 없을때만 친구 목록 정렬을 명령합니다.
        /// </remarks>
        partial void OnRoomTitleChanged(string value)
        {
            // 글자수 검사
            ValidateRoomTitle();
            // 글자가 변경될때마다 CreateChatRoomCommand의 CanExecute 갱신하도록 이벤트 등록
            CreateChatRoomCommand.NotifyCanExecuteChanged();
        }
        #endregion OnChanged Method
        #region private Method
        /// <summary>
        /// 사용자 입력 칸들을 초기화합니다.
        /// </summary>
        private void ResetInputValues()
        {
            RoomTitle = string.Empty;
            IsTitleEditEnabled = true;
            TitleWarningText = string.Empty;
            InviteWarningText = string.Empty;
            _targetRoomId = Guid.Empty;
            _profileIMGURL = null;
        }
        /// <summary>
        /// 현재 입력된 방 제목의 글자 수를 검증하고 경고 메시지를 제어합니다.
        /// </summary>
        private void ValidateRoomTitle()
        {
            // 기존 그룹 채팅방 인원 추가 모드일 때는 방 제목 규칙 검증을 면제합니다.
            if (!IsTitleEditEnabled)
            {
                TitleWarningText = string.Empty;
                return;
            }

            // 최초 진입 시 string.Empty이거나 사용자가 Clear 버튼을 눌러 null이 된 경우 가드
            if (string.IsNullOrEmpty(RoomTitle) || RoomTitle.Length < 4)
            {
                TitleWarningText = "방 제목은 최소 4글자 이상이어야 합니다.";
            }
            else if (RoomTitle.Length > 24)
            {
                TitleWarningText = "방 제목은 최대 24글자까지 가능합니다.";
            }
            else
            {
                TitleWarningText = string.Empty; // 정상 범위일 경우 경고 지움
            }
        }
        /// <summary>
        /// 서버로부터 친구 목록을 받아옵니다.
        /// </summary>
        /// <returns></returns>
        private async Task LoadFriendsAsync()
        {
            try
            {
                // FriendListViewModel의 LoadFriendsAsync와 동일한 서비스 호출
                ServiceResult<List<FriendModel>> response = await _friendService.GetFriendsListAsync();
                if (!response.IsSuccess) return;

                FriendList.Clear();
                foreach (FriendModel friend in response.Data)
                {
                    // IsSelected 상태가 변경되면 CreateChatRoomCommand의 CanExecute 갱신하도록 이벤트 등록
                    friend.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(FriendModel.IsSelected))
                        {
                            // 초대할 친구를 선택했는데 채팅방 참가자 수가 99명을 초과하게되면 return
                            if (friend.IsSelected && TotalSelectedCount > 99)
                            {
                                friend.IsSelected = false;
                                InviteWarningText = "채팅방에는 최대 100명까지 입장 가능합니다.";
                                return;
                            }
                            RefreshCountAndCommand();
                        }
                    };
                    FriendList.Add(friend);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{nameof(CreateChatRoomViewModel)}_{nameof(LoadFriendsAsync)}]  Error: {ex.Message}");
            }
        }
        /// <summary>
        /// 선택된 인원수 가상 프로퍼티와 초대 버튼의 활성화 상태를 실시간으로 새로고침합니다.
        /// </summary>
        private void RefreshCountAndCommand()
        {
            OnPropertyChanged(nameof(TotalSelectedCount));
            OnPropertyChanged(nameof(NewInvitedCount));

            // 1. 입력받은 참가자 정보가 있었는지 확인
            bool hasUser = FriendList.Any(f => !f.IsCheckBoxEnabled);

            // 1-1. 기존 그룹 채팅방에 신규 참가자를 추가하는 경우
            if (_targetRoomId != Guid.Empty)
            {
                if (NewInvitedCount == 0)
                    InviteWarningText = "초대할 친구를 1명 이상 선택해야 합니다.";
                else
                    InviteWarningText = string.Empty; // 한 명이라도 선택했으면 진행 가능
            }
            // 1-2. 기존 개인 채팅방에서 친구 초대를 누른 경우
            else if (hasUser)
            {
                if (NewInvitedCount == 0)
                    InviteWarningText = "초대할 친구를 1명 이상 선택해야 합니다.";
                else
                    InviteWarningText = string.Empty;
            }
            // 1-3. 그룹 채팅방 생성 버튼을 눌러서 진입한 경우
            else
            {
                if (NewInvitedCount < 2)
                    InviteWarningText = "그룹 채팅방 생성을 위해 친구를 2명 이상 선택해야 합니다.";
                else
                    InviteWarningText = string.Empty;
            }
            CreateChatRoomCommand.NotifyCanExecuteChanged();
        }
        /// <summary>
        /// 이미 채팅방에 존재하는 친구들은 선택이 불가능하게 상태를 변경합니다.
        /// </summary>
        /// <param name="participantEmails">이미 채팅방에 존재하는 친구들의 EmailList</param>
        private void UpdateParticipantStatus(List<string> participantEmails)
        {
            foreach (FriendModel friend in FriendList)
            {
                if (participantEmails.Contains(friend.Email))
                {
                    friend.IsSelected = true; // 체크박스 선택 상태로 변경
                    friend.IsCheckBoxEnabled = false; // 체크박스 접근 불가능하게 설정
                }
            }
        }
        /// <summary>
        /// DTO에 담겨있는 기존 채팅방 데이터로 View를 세팅합니다.
        /// </summary>
        /// <param name="message"></param>
        private void SetExsitingRoomData(OpenCreateChatRoomRequestMessage message)
        {
            _targetRoomId = message.RoomId!.Value;
            RoomTitle = message.Title!;
            _profileIMGURL = message.ProfileIMGURL;
            IsTitleEditEnabled = false;
        }
        /// <summary>
        /// 메서드가 실행되기전에 조건에 부합하는지 확인합니다.
        /// </summary>
        /// <returns></returns>
        private bool CanCreateChatRoom()
        {
            // 1. 방의 최대 인원수 (100명) 제한
            if (TotalSelectedCount > 99) return false;
            // 2. 기존 그룹 채팅에 신규 참가자 초대인 경우
            if (_targetRoomId != Guid.Empty)
                // 2-1. 무조건 초대하려는 인원이 1명 이상이어야 함
                return NewInvitedCount > 0;

            // 3. 신규 채팅방 개설인 경우
            if (string.IsNullOrEmpty(RoomTitle)) return false;
            bool isTitleValid = RoomTitle.Length > 3 && RoomTitle.Length < 25;
            if (!isTitleValid) return false;

            // 4. 입력받은 참가자 Email이 있었는지 확인
            bool hasUser = FriendList.Any(f => !f.IsCheckBoxEnabled);
            // 4-1. 입력받은 참가자가 있었으면 1명 이상 선택해야함
            if (hasUser)
                return NewInvitedCount >= 1;
            // 4-2. 입력받은 참가자가 없었으면 2명 이상 선택해야함
            else
                return NewInvitedCount >= 2;
        }
        /// <summary>
        /// 새로운 채팅방을 생성합니다.
        /// </summary>
        private async Task CreateNewChatRoom()
        {
            // 1. 초대하려는 참가자 정보 가져오기
            List<string>? selectedEmails = FriendList
                .Where(f => f.IsSelected)
                .Select(f => f.Email)
                .ToList();
            if (selectedEmails == null || selectedEmails.Count == 0) return;

            // 2. 그룹 채팅방 생성
            ServiceResult<Guid> response = await _chatService.CreateGroupChatAsync(RoomTitle, null, selectedEmails);
            if (response.Data == Guid.Empty) return;

            // 3. ChatRoom 화면으로 전환 요청
            WeakReferenceMessenger.Default.Send(new ChatRoomSelectionChangedMessage(response.Data));
        }
        /// <summary>
        /// 기존 채팅방에 새로운 참가자를 초대합니다.
        /// </summary>
        private async Task InviteNewChatParticipant()
        {
            // 1. 초대하려는 참가자 정보 가져오기
            List<string> inviteEmails = FriendList
                    .Where(f => f.IsSelected && f.IsCheckBoxEnabled)
                    .Select(f => f.Email)
                    .ToList();
            if (inviteEmails.Count == 0) return;

            // 2. 기존 채팅방에 신규 참가자 초대
            ServiceResult<bool> response = await _chatService.InviteParticipantsAsync(_targetRoomId, inviteEmails);
            if (!response.IsSuccess) return;

            // 3. 초대 완료되면 해당 채팅방으로 화면 전환
            WeakReferenceMessenger.Default.Send(new ChatRoomSelectionChangedMessage(_targetRoomId));
        }
        #endregion private Method
    }
}
