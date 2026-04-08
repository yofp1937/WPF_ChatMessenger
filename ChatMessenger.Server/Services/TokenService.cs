/*
 * JWT 토큰 생성 기능을 담당하는 서비스 클래스입니다.
 */
using ChatMessenger.Server.Interfaces;
using ChatMessenger.Server.Data.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatMessenger.Server.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <inheritdoc/>
        public string CreateToken(User user)
        {
            // appsettings.json에서 값 가져오기
            string jwtKey = _config["Jwt:Key"] ?? throw new Exception("JWT Key missing");
            // 컴퓨터가 사용할수있도록 바이트 배열로 변경
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtKey));

            // 토큰에 기본 정보 삽입
            List<Claim> claims = new()
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email), // Sub: 토큰의 주인
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Jti: 토큰의 고유 번호
                new Claim("Nickname", user.NickName) // 커스텀 데이터 삽입
            };

            // 어떤 알고리즘(HmacHsa256)으로 key를 암호화해서 서명할것인지
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

            // 토큰 명세서 작성
            SecurityTokenDescriptor tokenDescriptor = new()
            {
                Subject = new ClaimsIdentity(claims), // 기본 정보 등록
                Expires = DateTime.Now.AddHours(8), // 유효 시간 설정
                SigningCredentials = creds, // 서명
                Issuer = _config["Jwt:Issuer"], // 발급자 (서버)
                Audience = _config["Jwt:Audience"] // 수신자 (클라이언트)
            };

            // 토큰 관리자 생성
            JwtSecurityTokenHandler tokenHandler = new();
            // 토큰 관리자에게 명세서 보여주면서 토큰 생성 요청
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            // 객체 데이터를 Base64 인코딩 문자열로 변경하여 반환(Client에서 HTTP Header에 넣어서 전송할수있도록)
            return tokenHandler.WriteToken(token);
        }
    }
}
