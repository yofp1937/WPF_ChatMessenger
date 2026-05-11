using ChatMessenger.Server.Data.Entities;

namespace ChatMessenger.Server.Interfaces.Auth
{
    /// <summary>
    /// JWT 토큰 생성 로직을 처리하는 Service의 Interface입니다.
    /// </summary>
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
