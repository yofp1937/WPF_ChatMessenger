using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Server.Mappers
{
    /// <summary>
    /// AuthService에서 사용되는 DTO들의 Mapper 클래스입니다. <br/>
    /// Controller와 Service 사이에서 데이터를 목적에 적합한 형태로 가공해줍니다.
    /// </summary>
    public static class AuthMapper
    {
        /// <summary>
        /// 로그인에 성공했을때 User 엔티티를 LoginResponse DTO로 변환해줍니다.
        /// </summary>
        /// <param name="user">로그인에 성공한 user의 정보</param>
        /// <param name="token">로그인에 성공한 user에게 발행된 토큰</param>
        /// <returns>Client에게 전송해줄 LoginResponse</returns>
        public static LoginResponse ToLoginResponse(User user, string token)
        {
            return new LoginResponse
            {
                IsSuccess = true,
                Token = token,
                Message = "로그인에 성공했습니다.",
                UserProfile = user.MapToFriendResponse(isMe: true)
            };
        }
        /// <summary>
        /// 로그인에 실패했을때의 LoginResponse를 생성합니다.
        /// </summary>
        /// <param name="message">Client에게 반환해줄 메세지</param>
        /// <returns>로그인 실패 결과가 담긴 LoginResponse</returns>
        public static LoginResponse ToFailLoginResponse(string message)
        {
            return new LoginResponse
            {
                IsSuccess = false,
                Message = message,
            };
        }
        /// <summary>
        /// 회원가입 결과 RegisterResponse를 생성합니다.
        /// </summary>
        /// <param name="isSuccess">회원가입 성공 여부</param>
        /// <param name="message">Client에게 반환해줄 메세지</param>
        /// <returns>회원가입 결과가 담긴 RegisterResponse</returns>
        public static RegisterResponse ToRegisterResponse(bool isSuccess, string message)
        {
            return new RegisterResponse
            {
                IsSuccess = isSuccess,
                Message = message,
            };
        }
    }
}
