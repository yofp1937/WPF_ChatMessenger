/*
 * JWT 설정을 담당하는 클래스
 */
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ChatMessenger.Server.Configs
{
    public static class JWTConfig
    {
        /// <summary>
        /// JWT 토큰 검증 로직을 구성하고 서비스를 등록합니다.
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            // appsettings.json의 Jwt 섹션에서 설정값들을 가져옵니다.
            string jwtKey = config["Jwt:Key"]!;
            string jwtIssuer = config["Jwt:Issuer"]!;
            string jwtAudience = config["Jwt:Audience"]!;

            // JwtBearer 방식의 서비스를 등록합니다.
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // 토큰 설정
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, // 토큰의 Issuer가 일치하는지 확인
                        ValidateAudience = true, // 토큰의 Audience가 일치하는지 확인
                        ValidateLifetime = true, // 유효기간 확인
                        ValidateIssuerSigningKey = true, // Server 내부에 설정된 jwtKey로 서명된건지 확인
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        // jwtKey를 암호화하여 입력해두고 나중에 이를통해 검증
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    };
                });
            return services;
        }
    }
}
