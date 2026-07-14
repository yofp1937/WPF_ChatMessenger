/*
 * Server에서 사용될 Service 등록을 담당하는 클래스
 * 컨트롤러의 생성자에서 자동으로 주입받아 사용할 각종 서비스 Class들을 DI Container에 등록합니다.
 */
using ChatMessenger.Server.Interfaces.Services;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Services;
using ChatMessenger.Server.Services.Repositories;

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
            // 비밀번호 해싱 서비스 등록 (DbContext 등 요청 단위 자원을 참조하지 않는 무상태 서비스이므로 Singleton으로 등록)
            services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
            // TODO: 나중에 Service 추가되면 이곳에 작성하여 추가
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ISocialService, SocialService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IChatRoomRepositoryService, ChatRoomRepositoryService>();
            services.AddScoped<IChatParticipantRepositoryService, ChatParticipantRepositoryService>();
            services.AddScoped<IChatMessageRepositoryService, ChatMessageRepositoryService>();
            services.AddScoped<IUserRepositoryService, UserRepositoryService>();
            services.AddScoped<IFriendshipRepository, FriendshipRepositoryService>();

            return services;
        }
    }
}
