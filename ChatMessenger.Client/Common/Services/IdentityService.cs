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
        private string? _token;
        private FriendModel? _myProfile;

        public string Token => _token! ??
            throw new InvalidOperationException("인증 토큰이 초기화되지 않았습니다. 로그인이 필요합니다.");
        public FriendModel MyProfile => _myProfile! ??
            throw new InvalidOperationException("사용자 프로필 정보가 초기화되지 않았습니다. 로그인이 필요합니다.");
        public bool IsLoggedIn => !string.IsNullOrEmpty(_token);

        /// <inheritdoc/>
        public void Initialize(string token, FriendModel myProfile)
        {
            // null이면 throw
            _token = token ?? throw new ArgumentNullException(nameof(token));
            _myProfile = myProfile ?? throw new ArgumentNullException(nameof(myProfile));
        }

        /// <inheritdoc/>
        public void Logout()
        {
            _token = null;
            _myProfile = null;
        }
    }
}
