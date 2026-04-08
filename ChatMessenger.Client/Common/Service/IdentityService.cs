/*
 * 로그인에 성공하여 Server로부터 받은 인증 정보를 메모리에 유지하는 클래스
 * 싱글톤으로 만들어서 사용합니다.
 */
using ChatMessenger.Client.Common.Interfaces;

namespace ChatMessenger.Client.Common.Service
{
    public class IdentityService : IIdentityService
    {
        public string? Token { get; set; }
        public string? CurrentUserEmail { get; set; }
        public string? Nickname { get; set; }
        public string? StatusMessage { get; set; }
        public string? ProfileImageURL { get; set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        /// <inheritdoc/>
        public void Logout()
        {
            Token = null;
            CurrentUserEmail = null;
            Nickname = null;
            StatusMessage = null;
            ProfileImageURL = null;
        }
    }
}
