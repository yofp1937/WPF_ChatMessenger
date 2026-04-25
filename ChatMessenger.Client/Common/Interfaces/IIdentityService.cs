/*
 * 로그인한 사용자의 인증 정보 및 세션을 관리하는 서비스 인터페이스
 */

using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IIdentityService
    {
        string? Token { get; set; }
        FriendModel? MyProfile { get; set; }
        bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        /// <summary>
        /// 로그아웃시 정보를 초기화합니다.
        /// </summary>
        void Logout();
    }
}
