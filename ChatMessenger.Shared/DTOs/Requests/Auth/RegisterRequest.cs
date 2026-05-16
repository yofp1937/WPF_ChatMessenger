/*
 * Client에서 Server로 Register를 위해 보내는 Data
 */
namespace ChatMessenger.Shared.DTOs.Requests.Auth
{
    /// <summary>
    /// Server에서 Client로 Register 요청에 대한 결과를 반환해주는 DTO입니다.
    /// </summary>
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
    }
}
