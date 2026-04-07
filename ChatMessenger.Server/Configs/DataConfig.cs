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
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // DB 초기화 서비스 등록
            services.AddScoped<IDbInitializer, DbInitializer>();

            return services;
        }
    }
}
