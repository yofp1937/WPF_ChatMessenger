using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Auth;
using ChatMessenger.Shared.DTOs.Responses.Auth;

namespace ChatMessenger.Server.Interfaces.Services
{
    /// <summary>
    /// 로그인과 회원가입에 관련된 로직을 처리하는 Service의 Interface입니다.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 전달받은 값이 유효한지 검증하고, 회원가입 요청을 처리합니다.
        /// </summary>
        /// <param name="request">회원가입 처리에 필요한 데이터가 담긴 RegisterRequest</param>
        /// <returns>요청에 대한 결과가 담긴 ServiceResult</returns>
        Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// 전달받은 값이 유효한지 검증하고, 로그인 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 로그인에 성공 시 토큰을 발행해줍니다.
        /// </remarks>
        /// <param name="request">로그인 처리에 필요한 데이터가 담긴 LoginRequest</param>
        /// <returns>요청에 대한 결과가 담긴 ServiceResult</returns>
        Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    }
}
