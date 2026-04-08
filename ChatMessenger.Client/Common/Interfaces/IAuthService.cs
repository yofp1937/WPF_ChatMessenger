/*
 * DB와 통신하여 로그인 인증을 담당 하는 Class가 구현해야할 Interface
 */
using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Client.Common.Interfaces
{
    public interface IAuthService
    {
        /// <summary>
        /// DB에 사용자 인증을 시도하고 성공 시 토큰을 반환합니다.
        /// </summary>
        /// <param name="email">사용자 이메일</param>
        /// <param name="password">사용자 비밀번호</param>
        /// <param name="nickname">사용자 별명</param>
        /// <returns>인증 성공시 LoginResponse DTO, 실패시 null</returns>
        Task<LoginResponse?> SignInAsync(string email, string password);

        /// <summary>
        /// DB에 회원가입을 요청합니다.
        /// </summary>
        Task<bool> RegisterAsync(string email, string password, string nickname);
    }
}
