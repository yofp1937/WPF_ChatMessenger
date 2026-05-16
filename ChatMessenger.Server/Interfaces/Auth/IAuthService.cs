using ChatMessenger.Shared.DTOs.Requests.Auth;
using ChatMessenger.Shared.DTOs.Responses.Auth;

namespace ChatMessenger.Server.Interfaces.Auth
{
    /// <summary>
    /// 로그인과 회원가입에 관련된 로직을 처리하는 Service의 Interface입니다.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 회원가입 요청을 처리합니다.
        /// </summary>
        /// <param name="request">회원가입 처리에 필요한 데이터가 담긴 RegisterRequest</param>
        /// <returns>회원가입 요청에대한 결과가 담긴 RegisterResponse</returns>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 로그인 요청을 처리합니다.
        /// </summary>
        /// <param name="request">로그인 처리에 필요한 데이터가 담긴 LoginRequest</param>
        /// <returns>로그인 요청에대한 결과가 담긴 LoginResponse</returns>
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
