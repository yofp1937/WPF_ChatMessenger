/*
 * 사용자 인증 로직(회원가입, 로그인)을 처리하는 서비스 클래스
 * 서버와 통신하여 JWT 토큰을 발급받거나 계정 생성을 요청합니다.
 */
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses;
using System.Net.Http;
using System.Net.Http.Json;

namespace ChatMessenger.Client.Common.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<LoginResponse?> SignInAsync(string email, string password)
        {
            try
            {
                // 1. 로그인용 DTO 생성
                LoginRequest requestData = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                // 2. 서버의 "api/auth/login" 주소로 POST 요청을 보냄 (HTTPClient를 사용해 객체를 자동으로 JSON으로 변환)
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/login", requestData);

                // 3. 서버가 반환한 상태 코드가 200(OK)인지 확인
                if (response.IsSuccessStatusCode)
                {
                    // Response의 JSON을 LoginResponse 객체로 역직렬화
                    return await response.Content.ReadFromJsonAsync<LoginResponse>();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthServcie - SignInAsync Error]: {ex.Message}");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> RegisterAsync(string email, string password, string nickname)
        {
            try
            {
                // 1. 회원가입용 DTO 생성
                RegisterRequest requestData = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    Nickname = nickname
                };

                // 2. 서버의 "api/auth/register"로 전송
                HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/register", requestData);

                // 3. 서버가 반환한 상태 코드가 200(OK)인지 확인
                if (response.IsSuccessStatusCode)
                {
                    // Response의 JSON을 RegisterResponse 객체로 역직렬화
                    RegisterResponse? result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
                    return result?.IsSuccess ?? false;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService - RegisterAsync Error]: {ex.Message}");
                return false;
            }
        }
    }
}
