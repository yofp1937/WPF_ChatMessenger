using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Services;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.DTOs.Requests.Auth;
using ChatMessenger.Shared.DTOs.Responses.Auth;
using ChatMessenger.Shared.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ChatMessenger.Server.Tests.Services
{
    /// <summary>
    /// 회원가입/로그인 비즈니스 로직을 처리하는 AuthService의 단위 테스트입니다.
    /// </summary>
    /// <remarks>
    /// IUserRepositoryService, ITokenService, IPasswordHasherService는 Moq으로 대체하여 AuthService의 로직만 독립적으로 검증합니다.
    /// ILogger는 검증 대상이 아니므로 NullLogger를 사용합니다.
    /// </remarks>
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepositoryService> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IPasswordHasherService> _passwordHasherMock;
        private readonly AuthService _sut; // System Under Test

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepositoryService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _passwordHasherMock = new Mock<IPasswordHasherService>();
            _sut = new AuthService(_userRepositoryMock.Object, _tokenServiceMock.Object, _passwordHasherMock.Object,
                NullLogger<AuthService>.Instance);
        }

        #region RegisterAsync
        [Fact]
        public async Task RegisterAsync_이메일이_비어있으면_BadRequest를_반환하고_저장을_시도하지_않는다()
        {
            // Arrange
            RegisterRequest request = new() { Email = "", Password = "1234", Nickname = "테스터" };

            // Act
            ServiceResult<RegisterResponse> result = await _sut.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultType.BadRequest, result.ResultType);
            _userRepositoryMock.Verify(r => r.AddNewUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_이미_가입된_이메일이면_BadRequest를_반환한다()
        {
            // Arrange
            RegisterRequest request = new() { Email = "exist@test.com", Password = "1234", Nickname = "테스터" };
            _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email)).ReturnsAsync(true);

            // Act
            ServiceResult<RegisterResponse> result = await _sut.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultType.BadRequest, result.ResultType);
            _userRepositoryMock.Verify(r => r.AddNewUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_정상_요청이면_평문이_아닌_해싱된_비밀번호로_저장한다()
        {
            // Arrange: R-3(비밀번호 평문 저장 → PBKDF2 해싱)에 대한 회귀 방지 테스트
            RegisterRequest request = new() { Email = "new@test.com", Password = "PlainPassword123!", Nickname = "테스터" };
            _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email)).ReturnsAsync(false);
            _passwordHasherMock.Setup(h => h.HashPassword(request.Password)).Returns("HASHED_VALUE");
            _userRepositoryMock.Setup(r => r.AddNewUserAsync(request.Email, "HASHED_VALUE", request.Nickname)).ReturnsAsync(true);

            // Act
            ServiceResult<RegisterResponse> result = await _sut.RegisterAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _passwordHasherMock.Verify(h => h.HashPassword(request.Password), Times.Once);
            // 평문 비밀번호를 그대로 저장하려 한 적이 없는지 확인 (R-3 회귀 방지의 핵심 검증)
            _userRepositoryMock.Verify(r => r.AddNewUserAsync(request.Email, request.Password, request.Nickname), Times.Never);
            _userRepositoryMock.Verify(r => r.AddNewUserAsync(request.Email, "HASHED_VALUE", request.Nickname), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_저장에_실패하면_InternalServerError를_반환한다()
        {
            // Arrange
            RegisterRequest request = new() { Email = "new@test.com", Password = "1234", Nickname = "테스터" };
            _userRepositoryMock.Setup(r => r.FindUserByEmailAsync(request.Email)).ReturnsAsync(false);
            _passwordHasherMock.Setup(h => h.HashPassword(request.Password)).Returns("HASHED_VALUE");
            _userRepositoryMock.Setup(r => r.AddNewUserAsync(request.Email, "HASHED_VALUE", request.Nickname)).ReturnsAsync(false);

            // Act
            ServiceResult<RegisterResponse> result = await _sut.RegisterAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultType.InternalServerError, result.ResultType);
        }
        #endregion RegisterAsync

        #region LoginAsync
        [Fact]
        public async Task LoginAsync_존재하지_않는_이메일이면_BadRequest를_반환한다()
        {
            // Arrange
            LoginRequest request = new() { Email = "notfound@test.com", Password = "1234" };
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(request.Email)).ReturnsAsync((User?)null);

            // Act
            ServiceResult<LoginResponse> result = await _sut.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultType.BadRequest, result.ResultType);
        }

        [Fact]
        public async Task LoginAsync_비밀번호가_일치하지_않으면_BadRequest를_반환하고_토큰을_발급하지_않는다()
        {
            // Arrange: R-3 회귀 방지 — 예전 평문 비교(user.Password != request.Password) 로직으로 되돌아가면
            // VerifyPassword가 호출되지 않으므로 아래 Verify에서 실패함
            User user = new() { Email = "user@test.com", Password = "HASHED_VALUE", Nickname = "테스터" };
            LoginRequest request = new() { Email = user.Email, Password = "WrongPassword" };
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword(request.Password, user.Password)).Returns(false);

            // Act
            ServiceResult<LoginResponse> result = await _sut.LoginAsync(request);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(ServiceResultType.BadRequest, result.ResultType);
            _passwordHasherMock.Verify(h => h.VerifyPassword(request.Password, user.Password), Times.Once);
            _tokenServiceMock.Verify(t => t.CreateToken(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task LoginAsync_비밀번호가_일치하면_토큰을_발급하고_성공을_반환한다()
        {
            // Arrange
            User user = new() { Email = "user@test.com", Password = "HASHED_VALUE", Nickname = "테스터" };
            LoginRequest request = new() { Email = user.Email, Password = "CorrectPassword" };
            _userRepositoryMock.Setup(r => r.GetUserByEmailAsync(request.Email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(h => h.VerifyPassword(request.Password, user.Password)).Returns(true);
            _tokenServiceMock.Setup(t => t.CreateToken(user)).Returns("FAKE_JWT_TOKEN");

            // Act
            ServiceResult<LoginResponse> result = await _sut.LoginAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("FAKE_JWT_TOKEN", result.Data.Token);
            _tokenServiceMock.Verify(t => t.CreateToken(user), Times.Once);
        }
        #endregion LoginAsync
    }
}
