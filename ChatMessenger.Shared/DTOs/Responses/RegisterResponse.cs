/*
 * Server에서 Client에게 Register 요청에 대한 결과를 반환해주는 Data
 */
namespace ChatMessenger.Shared.DTOs.Responses
{
    /// <summary>
    /// Client에서 Server로 회원가입을 요청할때 전달하는 DTO입니다.
    /// </summary>
    public class RegisterResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
