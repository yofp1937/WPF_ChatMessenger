using ChatMessenger.Server.Services;

namespace ChatMessenger.Server.Tests.Services
{
    /// <summary>
    /// PBKDF2 기반 PasswordHasherService의 해싱/검증 로직을 검증하는 단위 테스트입니다.
    /// </summary>
    /// <remarks>
    /// PasswordHasherService는 외부 의존성이 전혀 없는 순수 클래스이므로 Mock 없이 직접 인스턴스를 생성해 테스트합니다.
    /// </remarks>
    public class PasswordHasherServiceTests
    {
        private readonly PasswordHasherService _sut; // System Under Test

        public PasswordHasherServiceTests()
        {
            _sut = new PasswordHasherService();
        }

        [Fact]
        public void HashPassword_같은_비밀번호를_두번_해싱하면_서로_다른_결과가_나온다()
        {
            // Arrange
            string password = "MySecurePassword123!";

            // Act
            string hash1 = _sut.HashPassword(password);
            string hash2 = _sut.HashPassword(password);

            // Assert: 매번 새로운 Salt를 사용하므로 같은 비밀번호라도 해시 결과는 달라야 함 (Rainbow Table 공격 방어 확인)
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void VerifyPassword_해싱에_사용한_원래_비밀번호로_검증하면_true를_반환한다()
        {
            // Arrange
            string password = "MySecurePassword123!";
            string hash = _sut.HashPassword(password);

            // Act
            bool result = _sut.VerifyPassword(password, hash);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void VerifyPassword_틀린_비밀번호로_검증하면_false를_반환한다()
        {
            // Arrange
            string hash = _sut.HashPassword("MySecurePassword123!");

            // Act
            bool result = _sut.VerifyPassword("WrongPassword!", hash);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void VerifyPassword_Base64_형식이_아닌_손상된_문자열이_들어오면_예외없이_false를_반환한다()
        {
            // Arrange: DbInitializer가 예전에 평문("1")을 그대로 저장했던 것과 같은 오염 시나리오를 재현
            string corrupted = "이건_해시가_아닌_평문입니다";

            // Act
            bool result = _sut.VerifyPassword("아무_비밀번호", corrupted);

            // Assert
            Assert.False(result);
        }
    }
}
