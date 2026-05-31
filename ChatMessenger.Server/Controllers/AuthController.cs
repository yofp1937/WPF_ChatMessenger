using ChatMessenger.Server.Controllers.Base;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Auth;
using ChatMessenger.Shared.DTOs.Responses.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ChatMessenger.Server.Controllers
{
    /// <summary>
    /// 사용자 인증(로그인, 회원가입)과 관련된 HTTP 요청을 처리하는 Controller입니다.
    /// </summary>
    [Route("api/[controller]")] // 주소: "https://서버주소/api/auth" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class AuthController : AnonymousBaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) : base()
        {
            _authService = authService;
        }

        /// <summary>
        /// 로그인 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 로그인 성공 시: HTTP 응답 코드 200(OK)과 함께 토큰 및 프로필 정보 반환<br/>
        /// 로그인 실패 시: HTTP 응답 코드 401(Unauthorized)과 함께 에러 메세지 반환
        /// </remarks>
        [HttpPost("login")] // Post 형식이므로 주소창에 정보가 노출되지 않음
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // 1. authService를 통해 로그인을 시도하고 결과값 전달받음
            ServiceResult<LoginResponse> response = await _authService.LoginAsync(request);
            return ContextResponse(response);
        }

        /// <summary>
        /// 회원가입 요청을 처리합니다.
        /// </summary>
        /// <remarks>
        /// 로그인 성공 시: HTTP 응답 코드 200(OK) 반환<br/>
        /// 로그인 실패 시: HTTP 응답 코드 400(BadRequest)과 함께 에러 메세지 반환
        /// </remarks>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. authService를 통해 회원가입을 시도하고 결과값 전달받음
            ServiceResult<RegisterResponse> response = await _authService.RegisterAsync(request);
            return ContextResponse(response);
        }
    }
}
