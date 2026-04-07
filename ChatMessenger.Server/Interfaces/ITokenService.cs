/*
 * JWT 토큰 생성을 담당하는 클래스들이 구현해야하는 Interface
 */
using ChatMessenger.Shared.Models;

namespace ChatMessenger.Server.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// JWT Token에 유저 정보를 삽입하여 생성합니다.
        /// </summary>
        /// <param name="user">Token에 포함시킬 User 정보</param>
        /// <returns>Base64로 인코딩된 JWT 문자열</returns>
        string CreateToken(User user);
    }
}
