/*
 * IDbInitializer를 구현하여
 * MSSQL의 연결과 필수 데이터 초기 생성까지 담당하는 클래스입니다.
 * 
 * 1.외부에서 InitializeDbAsync() 호출하면 일회용 Scope를 생성함
 * 2.Scope 내부에서 Database 없으면 생성하고, AppDbContext에 등록된 Table 확인하고 없으면 생성함
 * 3.Test Data까지 주입하고 Scope 종료함
 */
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Data
{
    public class DbInitializer : IDbInitializer
    {
        #region Property
        private readonly IServiceScopeFactory _scopeFactory;
        #endregion

        #region 생성자
        public DbInitializer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        #endregion

        #region public Method
        /// <inheritdoc/>
        public async Task InitializeDbAsync()
        {
            // 1.DB와 연결 통로 생성
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Console.WriteLine("[DbInitializer - InitializeDbAsync]: Db를 초기화합니다.");

                // 2.DB 연결 및 생성, 테스트 테이블 입력까지 처리
                if (await ConnectionDbAsync(context))
                {
                    await SeedTestDataAsync(context);
                }
            }
        }
        /// <inheritdoc/>
        public async Task<bool> ConnectionDbAsync(AppDbContext context)
        {
            // DB가 없으면 생성해주는 CheckAndCreateTablesAsync() 호출
            await CheckAndCreateTablesAsync(context);
            try
            {
                Console.WriteLine("[DbInitializer - ConnectionDbAsync]: Db 연결 확인 중...");
                if (await context.Database.CanConnectAsync())
                {
                    string dbName = context.Database.GetDbConnection().Database;
                    Console.WriteLine($"[DbInitializer - ConnectionDbAsync]: MSSQL 서버 {dbName} 데이터베이스에 연결됐습니다.");
                    return true;
                }
                else
                {
                    Console.WriteLine("[DbInitializer - ConnectionDbAsync]: 연결이 거부됐습니다. 서버 설정을 확인하세요.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer - ConnectionDbAsync]: 에러 발생 {ex.Message}");
                return false;
            }
        }
        /// <inheritdoc/>
        public async Task CheckAndCreateTablesAsync(AppDbContext context)
        {
            Console.WriteLine("[DbInitializer - CheckAndCreateTablesAsync]: 데이터베이스와 테이블 구조 확인 및 생성 중...");
            /* EnsureCreatedAsync를 사용해서 Database와 Table이 존재하지 않으면 생성
             * 1.context를 생성할때 매개변수로 넣은 option의 database가 존재하는지 확인하고 없으면 생성
             * 2.context 객체 내부에 DbSet으로 선언된 Property들의 정보대로 Table들 존재하는지 확인하고 없으면 생성 */
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("[DbInitializer - CheckAndCreateTablesAsync]: 데이터베이스 구조가 준비되었습니다.");
        }
        /// <inheritdoc/>
        public async Task SeedTestDataAsync(AppDbContext context)
        {
            if (!await context.Users.AnyAsync())
            {
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 초기 테스트 데이터 주입 중...");
                User testUser = new User
                {
                    Email = "test@test.com",
                    Password = "1234",
                    NickName = "테스터",
                };
                context.Users.Add(testUser);
                await context.SaveChangesAsync();
                Console.WriteLine($"[DbInitializer - SeedTestDataAsync]: 테스트 데이터({testUser})가 생성되었습니다.");
            }
            else
            {
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 이미 데이터가 존재하므로 시딩을 건너뜁니다.");
            }
        }
        #endregion
    }
}
