using ChatMessenger.Server.Interfaces.Services;
using System.Security.Cryptography;

namespace ChatMessenger.Server.Services
{
    /// <summary>
    /// PBKDF2(Rfc2898DeriveBytes) 알고리즘으로 비밀번호를 해싱/검증하는 Service입니다.
    /// </summary>
    /// <remarks>
    /// System.Security.Cryptography는 .NET 기본 프레임워크에 포함돼있어 별도 NuGet 패키지가 필요하지 않습니다.<br/>
    /// 해시 결과 안에 반복 횟수(IterationCount)와 Salt를 함께 저장하므로, 추후 IterationCount를 상향 조정해도
    /// 기존에 저장돼있던 예전 해시를 그대로 검증할 수 있습니다.
    /// </remarks>
    public class PasswordHasherService : IPasswordHasherService
    {
        private const int SaltSize = 16;               // 128bit Salt
        private const int KeySize = 32;                // 256bit 파생 키(해시 결과) 길이
        private const int IterationCount = 100_000;    // OWASP 권장 PBKDF2-HMAC-SHA256 최소 반복 횟수
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        #region public Method
        /// <inheritdoc/>
        public string HashPassword(string password)
        {
            // 1. 매번 새로운 무작위 Salt 생성 (동일 비밀번호라도 해시 결과가 매번 달라지게하여 Rainbow Table 공격을 무력화)
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            // 2. Salt와 반복 횟수를 적용해 파생 키(해시) 생성
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, IterationCount, Algorithm, KeySize);
            // 3. [반복 횟수(4byte)][Salt(16byte)][파생 키(32byte)] 순서로 하나의 byte 배열에 결합
            byte[] result = new byte[sizeof(int) + SaltSize + KeySize];
            BitConverter.GetBytes(IterationCount).CopyTo(result, 0);
            salt.CopyTo(result, sizeof(int));
            key.CopyTo(result, sizeof(int) + SaltSize);
            // 4. Db 컬럼(string)에 저장할 수 있도록 Base64 문자열로 인코딩하여 반환
            return Convert.ToBase64String(result);
        }
        /// <inheritdoc/>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashBytes;
            try
            {
                // 1. 저장돼있던 해시 문자열을 다시 byte 배열로 복원
                hashBytes = Convert.FromBase64String(hashedPassword);
            }
            catch (FormatException)
            {
                // Db에 저장된 값이 Base64 형식이 아니면(예: 예전 평문 데이터, 손상된 값) 검증 실패로 처리
                return false;
            }
            if (hashBytes.Length != sizeof(int) + SaltSize + KeySize)
                return false;
            // 2. 해시 생성 당시 사용했던 반복 횟수와 Salt 추출
            int iterationCount = BitConverter.ToInt32(hashBytes, 0);
            byte[] salt = hashBytes[sizeof(int)..(sizeof(int) + SaltSize)];
            byte[] storedKey = hashBytes[(sizeof(int) + SaltSize)..];
            // 3. 입력받은 평문 비밀번호를 동일한 Salt/반복 횟수로 재해싱
            byte[] computedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterationCount, Algorithm, KeySize);
            // 4. 타이밍 공격(Timing Attack) 방지를 위해 고정 시간 비교 사용
            //    (문자열 == 비교는 앞자리부터 다르면 즉시 종료되어, 응답 시간 차이로 정보가 유출될 수 있음)
            return CryptographicOperations.FixedTimeEquals(storedKey, computedKey);
        }
        #endregion public Method
    }
}
