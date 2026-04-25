/*
 * FriendView에서 표시될 친구의 정보를 담고있는 모델(내 정보도 포함)
 */
using ChatMessenger.Shared.DTOs.Responses;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.Models.Friends
{
    public partial class FriendModel : ObservableObject
    {
        // 고유 식별자는 변하지 않으니 public
        public string Email { get; set; } = string.Empty;

        [ObservableProperty]
        private string _nickname = string.Empty;
        [ObservableProperty]
        private string _statusMessage = string.Empty;
        [ObservableProperty]
        private string _profileImageURL = string.Empty;

        [ObservableProperty]
        private bool _isMe;
        [ObservableProperty]
        private bool _isAdded;
        [ObservableProperty]
        private bool _isBlocked;
        [ObservableProperty]
        private bool _isFavorite;

        public FriendModel() { }
        /// <summary>
        /// 오버로딩을 활용해 객체를 생성할때 DTO를 넣어주면 자동으로 매핑해줍니다.
        /// </summary>
        /// <param name="dto"></param>
        public FriendModel(FriendResponse dto)
        {
            if (dto == null) return;
            UpdateFromDTO(dto);
        }

        /// <summary>
        /// 서버에게 받은 DTO 데이터를 바탕으로 모델의 상태를 업데이트합니다
        /// </summary>
        /// <remarks>
        /// ※ 외부에서 해당 메서드로 데이터를 업데이트할땐 반드시 Email이 일치하는지 확인하고 업데이트해야합니다.
        /// </remarks>
        /// <param name="dto">서버 응답 DTO</param>
        public void UpdateFromDTO(FriendResponse dto)
        {
            if (dto == null) return;
            this.Email = dto.Email;
            this.Nickname = dto.Nickname;
            this.StatusMessage = dto.StatusMessage;
            this.ProfileImageURL = dto.ProfileImageURL;
            this.IsMe = dto.IsMe;
            this.IsAdded = dto.IsAdded;
            this.IsBlocked = dto.IsBlocked;
            this.IsFavorite = dto.IsFavorite;
        }
    }
}
