/*
 * 데이터베이스 연결 및 초기화 서비스 등록을 담당하는 클래스
 */
using ChatMessenger.Server.Data;
using ChatMessenger.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Configs
{
    public static class DataConfig
    {
        /// <summary>
        /// 데이터베이스 관련 핵심 서비스들을 IServiceCollection에 추가합니다.
        /// </summary>
        public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration config)
        {
            // appsettings.json에서 DB 주소 가져오고 DBContext 등록
            string? connectionString = config.GetConnectionString("DefaultConnection");

            // 운영 환경의 연결 문자열은 appsettings.Production.json에 커밋하지 않고 환경변수로만 주입하므로,
            // 값이 비어있으면 DB 연결 실패 원인을 알기 어려운 예외 대신 시작 시점에 명확히 실패시킴
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection 설정값이 비어있습니다. 운영 배포 시 환경변수 ConnectionStrings__DefaultConnection을 설정하세요.");

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // DB 초기화 서비스 등록
            services.AddScoped<IDbInitializer, DbInitializer>();

            return services;
        }
    }
}
