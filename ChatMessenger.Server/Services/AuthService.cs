using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Auth;
using ChatMessenger.Shared.DTOs.Responses.Auth;
using ChatMessenger.Shared.Enums;

namespace ChatMessenger.Server.Services
{
    /// <summary>
    /// AuthController의 요청에따라 회원가입, 로그인과 관련된 요청을 처리해주는 BusinessService입니다.
    /// </summary>
    public class AuthService : BaseBusinessService, IAuthService
    {
        private readonly IUserRepositoryService _userRepository;
        private readonly ITokenService _tokenService;

        public AuthService(IUserRepositoryService userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<ServiceResult<RegisterResponse>> RegisterAsync(RegisterRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 전달받은 데이터 검증
                if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Nickname))
                    return ServiceResult<RegisterResponse>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);
                // 2. 이미 가입된 이메일인지 체크
                bool hasData = await _userRepository.FindUserByEmailAsync(request.Email);
                if (hasData)
                    return ServiceResult<RegisterResponse>.Failed("사용중인 이메일입니다.", ServiceResultType.BadRequest);
                // 3. 회원가입 진행 (Db에 데이터 삽입)
                bool isRegistered = await _userRepository.AddNewUserAsync(request.Email, request.Password, request.Nickname);
                if (!isRegistered)
                    return ServiceResult<RegisterResponse>.Failed("회원 등록 중 서버 오류가 발생했습니다.", ServiceResultType.InternalServerError);
                // 4. Response 매핑하여 반환
                RegisterResponse response = AuthMapper.ToRegisterResponse(true, "회원가입에 성공하였습니다.");
                return ServiceResult<RegisterResponse>.Success(response);
            });
        }
        /// <inheritdoc/>
        public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request)
        {
            return await ExecutedBusinessLogicAsync(async () =>
            {
                // 1. 전달받은 데이터 검증
                if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                    return ServiceResult<LoginResponse>.Failed("유효한 요청 값이 아닙니다.", ServiceResultType.BadRequest);

                // 2. 로그인을 시도하는 Email의 유저 정보 찾기
                User? user = await _userRepository.GetUserByEmailAsync(request.Email);
                if (user == null || user.Password != request.Password)
                    return ServiceResult<LoginResponse>.Failed("이메일 혹은 비밀번호를 정확하게 입력해주세요.", ServiceResultType.BadRequest);

                // 3. 로그인 성공시 토큰 발행하고 Response 매핑하여 반환
                string token = _tokenService.CreateToken(user);
                LoginResponse response = AuthMapper.ToLoginResponse(user, token);
                return ServiceResult<LoginResponse>.Success(response);
            });
        }
        #endregion public Method
    }
}
