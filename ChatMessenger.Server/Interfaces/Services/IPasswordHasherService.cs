namespace ChatMessenger.Server.Interfaces.Services
{
    /// <summary>
    /// 비밀번호를 안전하게 해싱하고 검증하는 기능을 제공하는 Service입니다.
    /// </summary>
    public interface IPasswordHasherService
    {
        /// <summary>
        /// 평문 비밀번호를 해싱합니다.
        /// </summary>
        /// <param name="password">해싱할 평문 비밀번호</param>
        /// <returns>Db에 저장할 해시 문자열(Salt, 반복 횟수 포함)</returns>
        string HashPassword(string password);
        /// <summary>
        /// 평문 비밀번호가 저장된 해시와 일치하는지 검증합니다.
        /// </summary>
        /// <param name="password">검증하려는 평문 비밀번호</param>
        /// <param name="hashedPassword">Db에 저장돼있던 해시 문자열</param>
        /// <returns>일치하면 true, 아니면 false</returns>
        bool VerifyPassword(string password, string hashedPassword);
    }
}
