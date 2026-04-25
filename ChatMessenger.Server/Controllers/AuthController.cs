/*
 * Client에서 로그인, 회원가입 요청을 전송하면 이곳에서 처리합니다.
 */
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Extensions;
using ChatMessenger.Server.Interfaces;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Controllers
{
    [ApiController] // 웹 API 응답을 전담하는 Controller임을 선언
    [Route("api/[controller]")] // 주소: "https://서버주소/api/auth" (Class 이름에서 Contoller를 뺀 이름으로 자동 치환 됨)
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// 로그인 요청을 처리합니다.<br/>
        /// 주소: "https://서버주소/api/auth/login"
        /// </summary>
        [HttpPost("login")] // Post 형식이므로 주소창에 정보가 노출되지 않음
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // 반환타입 ActionResult<LoginResponse> - HTTP 상태코드와 함께 LoginResponse 데이터를 전송하겠다.
            // 매개변수 [FromBody] LoginRequest requst - Client가 전송한 Json 본문(Body)을 LoginRequest로 역직렬화하여 받겠다.
            // 1. 이메일(PK)로 유저 찾기
            User? user = await _context.Users.FindAsync(request.Email);

            // 2. 유저가 없거나 비밀번호가 틀린 경우
            if (user == null || user.Password != request.Password)
            {
                return Unauthorized(new LoginResponse // Unauthorized: HTTP 상태코드 401(미인증)을 반환
                {
                    IsSuccess = false,
                    Message = "이메일 또는 비밀번호가 일치하지 않습니다."
                });
            }

            // 3. 로그인 성공 (토큰은 임시로 생성)
            string token = _tokenService.CreateToken(user);
            return Ok(new LoginResponse
            {
                IsSuccess = true,
                Token = token,
                Message = "로그인에 성공했습니다.",
                UserProfile = user.MapToFriendResponse(isMe: true)
            });
        }

        /// <summary>
        /// 회원가입 요청을 처리합니다.<br/>
        /// 주소: "https://서버주소/api/auth/register"
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            // 1. 중복 체크 (Email이 PK이므로 이미 존재하면 에러)
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new RegisterResponse // BadRequest: HTTP 상태코드 400(잘못된 요청)을 반환
                {
                    IsSuccess = false,
                    Message = "이미 사용 중인 이메일입니다."
                });
            }

            // 2. 새 유저 생성
            var newUser = new User
            {
                Email = request.Email!,
                Password = request.Password!, // TODO: 나중에 암호화 필요
                Nickname = request.Nickname ?? "새 사용자"
            };

            Console.WriteLine($"{newUser.Email}님의 회원가입이 성공적으로 이루어졌습니다.");

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync(); // 실제 SQL INSERT문 실행됨

            return Ok(new RegisterResponse // OK: HTTP 상태코드 200(처리 성공)을 반환
            {
                IsSuccess = true,
                Message = "회원가입이 완료되었습니다."
            });
        }
    }
}
