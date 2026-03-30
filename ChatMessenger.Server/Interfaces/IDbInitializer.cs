/*
 * Database 연결을 담당하는 클래스들이 구현해야하는 Interface
 * 
 * 해당 인터페이스가 담당하는 핵심 기능은 Db와 연결이지만 초기화까지 담당해야해서 Initializer로 이름 정함
 */
using ChatMessenger.Server.Database;

namespace ChatMessenger.Server.Interfaces
{
    /// <summary>
    /// Db와 연결하고 테이블 초기화, 테스트 엔티티를 주입하는 역할도 담당합니다.
    /// </summary>
    public interface IDbInitializer
    {
        /// <summary>
        /// Db와의 연결, 필수 테이블 확인 및 체크, 테스트 데이터 주입을 순차적으로 실행하는 통합 메서드입니다.
        /// </summary>
        /// <returns></returns>
        Task InitializeDbAsync();

        /// <summary>
        /// Db와의 연결을 시도합니다.
        /// </summary>
        /// <returns>연결 성공시 True, 실패시 False</returns>
        Task<bool> ConnectionDbAsync(AppDbContext context);

        /// <summary>
        /// 필수 테이블이 존재하는지 확인하고 생성합니다.
        /// </summary>
        Task CheckAndCreateTablesAsync(AppDbContext context);

        /// <summary>
        /// 테스트 데이터를 주입합니다.
        /// </summary>
        /// <returns></returns>
        Task SeedTestDataAsync(AppDbContext context);
    }
}
