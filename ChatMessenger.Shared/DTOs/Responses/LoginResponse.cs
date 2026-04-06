/*
 * Server에서 Client에게 Login 요청에 대한 결과를 반환해주는 Data
 */
namespace ChatMessenger.Shared.DTOs.Responses
{
    /// <summary>
    /// Server에서 Client로 로그인 요청에 대한 결과를 반환해주는 DTO입니다.
    /// </summary>
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; }      // 성공 시 JWT 토큰
        public string? Message { get; set; } // 실패 시 에러 메시지
    }
}
