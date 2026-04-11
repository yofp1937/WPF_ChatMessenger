/*
 * HttpRequest의 Header에 Token을 자동으로 삽입해주는 핸들러입니다.
 */

using ChatMessenger.Client.Common.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ChatMessenger.Client.Common.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IIdentityService _identityService;

        public AuthHeaderHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        /// <summary>
        /// Server에게 HttpRequestMessage를 보낼때 자동으로 Header의 Authorization 칸에 Token을 삽입해줍니다.
        /// </summary>
        /// <param name="request">HttpRequest 요청</param>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1.메모리(IdentityService)에 토큰이 있는지 확인
            if (string.IsNullOrEmpty(_identityService.Token))
            {
                // 2.토큰이 없으면 서버로 요청을 전달하지않고 메서드 내에서 종료
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("인증 토큰이 없습니다. 로그인이 필요합니다."),
                    RequestMessage = request,
                };
            }
            // 3.토큰이 있으면 헤더에 토큰 삽입
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _identityService.Token);
            // 4.서버에 Request 요청
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
