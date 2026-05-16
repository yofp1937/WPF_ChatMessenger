/*
 * Server에서 Client에게 Login 요청에 대한 결과를 반환해주는 Data
 */
using ChatMessenger.Shared.DTOs.Responses.Friend;

namespace ChatMessenger.Shared.DTOs.Responses.Auth
{
    /// <summary>
    /// Server에서 Client로 로그인 요청에 대한 결과를 반환해주는 DTO입니다.
    /// </summary>
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? Token { get; set; } // 로그인 성공 시 JWT 토큰
        public string? Message { get; set; } // 실패 시 에러 메시지띄울 용도

        // 로그인한 유저의 정보도 전송
        public FriendResponse? UserProfile { get; set; }
    }
}
