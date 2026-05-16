/*
 * Server에서 사용될 Service 등록을 담당하는 클래스
 * 컨트롤러의 생성자에서 자동으로 주입받아 사용할 각종 서비스 Class들을 DI Container에 등록합니다.
 */
using ChatMessenger.Server.Common.Interfaces.Chats;
using ChatMessenger.Server.Common.Services.Chats;
using ChatMessenger.Server.Interfaces.Auth;
using ChatMessenger.Server.Interfaces.Chat;
using ChatMessenger.Server.Interfaces.Friend;
using ChatMessenger.Server.Services.Auth;
using ChatMessenger.Server.Services.Chat;
using ChatMessenger.Server.Services.Friend;

namespace ChatMessenger.Server.Configs
{
    public static class ServiceConfig
    {
        /// <summary>
        /// 다양한 Service들을 DI Container에 추가합니다.
        /// </summary>
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            // ★ 비즈니스 로직들은 대부분 AddScoped로 등록하여 요청 단위로 관리합니다.
            // 토큰 생성 서비스 등록
            services.AddScoped<ITokenService, TokenService>();
            // TODO: 나중에 Service 추가되면 이곳에 작성하여 추가
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFriendService, FriendService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IChatRoomService, ChatRoomService>();
            services.AddScoped<IChatParticipantService, ChatParticipantService>();
            services.AddScoped<IChatMessageService, ChatMessageService>();

            return services;
        }
    }
}
