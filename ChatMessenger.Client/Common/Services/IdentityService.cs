/*
 * 로그인에 성공하여 Server로부터 받은 인증 정보를 메모리에 유지하는 클래스
 * 싱글톤으로 만들어서 사용합니다.
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Services
{
    public class IdentityService : IIdentityService
    {
        public string? Token { get; set; }
        public FriendModel? MyProfile { get; set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        /// <inheritdoc/>
        public void Logout()
        {
            Token = null;
            MyProfile = null;
        }
    }
}
