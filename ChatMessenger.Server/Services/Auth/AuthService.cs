using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Auth;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Shared.DTOs.Requests.Auth;
using ChatMessenger.Shared.DTOs.Responses.Auth;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Auth
{
    /// <summary>
    /// AuthController의 요청에따라 회원가입, 로그인과 관련된 요청을 처리해주는 Service입니다.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // 1. 로그인을 시도하는 Email의 유저 정보 찾기
            User? user = await _context.Users.FindAsync(request.Email);
            // 2. 검증
            if (user == null)
            {
                return AuthMapper.ToFailLoginResponse("가입되지않은 이메일입니다.");
            }
            else if (user.Password != request.Password)
            {
                return AuthMapper.ToFailLoginResponse("이메일 혹은 비밀번호를 정확하게 입력해주세요.");
            }

            // 3. 로그인 성공시 토큰 발행
            string token = _tokenService.CreateToken(user);
            return AuthMapper.ToLoginResponse(user, token);
        }
        /// <inheritdoc/>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // 1. 이미 가입된 이메일인지 체크
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return AuthMapper.ToRegisterResponse(false, "이미 사용중인 이메일입니다.");

            try
            {
                // 2. 회원가입 진행 (Db에 데이터 삽입)
                await SaveUserToDbAsync(request);
                return AuthMapper.ToRegisterResponse(true, "회원가입에 성공하였습니다.");
            }
            catch (Exception ex)
            {
                // 3. Db 저장 중 오류 발생 시 처리
                // TODO: 나중에 실패 이유를 로그에 저장
                Console.WriteLine($"[{nameof(AuthService)}_{nameof(RegisterAsync)}]: {ex.Message}");
                return AuthMapper.ToRegisterResponse(false, "서버 오류로 회원가입에 실패했습니다.");
            }
        }
        #endregion public Method
        #region private Method
        /// <summary>
        /// Db에 새로운 사용자의 정보를 추가하고 저장합니다.
        /// </summary>
        /// <param name="request">사용자 정보 추가에 필요한 데이터가 담긴 RegisterRequest</param>
        private async Task SaveUserToDbAsync(RegisterRequest request)
        {
            User newUser = new()
            {
                Email = request.Email,
                // TODO: 나중에 패스워드 암호화를 추가하면 이부분 바꿔야함
                Password = request.Password,
                Nickname = request.Nickname,
            };
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }
        #endregion private Method
    }
}
