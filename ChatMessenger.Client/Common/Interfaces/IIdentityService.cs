/*
 * 로그인한 사용자의 인증 정보 및 세션을 관리하는 서비스 인터페이스
 */

using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IIdentityService
    {
        string Token { get; }
        FriendModel MyProfile { get; }
        bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        /// <summary>
        /// 발급받은 Token과 FriendModel 데이터를 저장해둡니다.
        /// </summary>
        /// <param name="token">로그인에 성공하여 Server로부터 발급받은 JWT 토큰</param>
        /// <param name="myProfile">나의 Profile을 표시할수있는 FriendModel 객체</param>
        void Initialize(string token, FriendModel myProfile);

        /// <summary>
        /// 로그아웃시 정보를 초기화합니다.
        /// </summary>
        void Logout();
    }
}
