/*
 * Client에서 Server로 Login을 위해 보내는 Data
 */
namespace ChatMessenger.Shared.DTOs.Requests.Auth
{
    /// <summary>
    /// Client에서 Server로 로그인 인증을 요청할때 전달하는 DTO입니다.
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
