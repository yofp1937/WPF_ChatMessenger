/*
 * 로그인에 성공하여 Server로부터 받은 인증 정보를 메모리에 유지하는 클래스
 * 싱글톤으로 만들어서 사용합니다.
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Messages;
using ChatMessenger.Client.ViewModels;
using CommunityToolkit.Mvvm.Messaging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatMessenger.Client.Common.Service
{
    public class IdentityService : IIdentityService
    {
        private string? _token;
        public string? Token
        {
            get => _token;
            set
            {
                _token = value;
                if(!string.IsNullOrEmpty(_token))
                {
                    DecodeToken(_token);
                }
            }
        }
        public string? CurrentUserEmail { get; set; }
        public string? Nickname { get; set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);

        /// <inheritdoc/>
        public void Logout()
        {
            Token = null;
            CurrentUserEmail = null;
            Nickname = null;
            WeakReferenceMessenger.Default.Send(new NavigationMessage(typeof(LoginViewModel)));
        }

        /// <summary>
        /// 서버로부터 Token을 받았을때 그 안에서 커스텀 데이터를 추출해줍니다.
        /// </summary>
        /// <param name="token"></param>
        private void DecodeToken(string token)
        {
            JwtSecurityTokenHandler handler = new();
            JwtSecurityToken jwtToken = handler.ReadJwtToken(token);

            // 서버에서 "Nickname"이라는 이름으로 Claim을 넣었으므로 그대로 찾아옵니다.
            Claim? nicknameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "Nickname");
            if (nicknameClaim != null)
            {
                Nickname = nicknameClaim.Value;
            }
        }
    }
}
