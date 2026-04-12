/*
 * Dependency Injection(DI) 컨테이너 구성을 위한 확장 메서드를 정의한 클래스입니다.
 * 
 * 프로젝트 내의 주요 구성 요소들을 서비스 컨테이너에 등록합니다.
 *  - 각종 Service
 *  - ViewModel과 Window 창을 필요로하는 View
 */
using ChatMessenger.Client.Common.Handlers;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Common.Services;
using ChatMessenger.Client.ViewModels.Windows;
using ChatMessenger.Client.ViewModels.Base;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Windows;

namespace ChatMessenger.Client.Configs
{
    public static class DependencyInjectionConfig
    {
        /// <summary>
        /// DI 컨테이너가 대신 관리할 객체들을 IServiceCollection에 등록합니다.<br/>
        /// 해당 메서드는 App.xaml.cs에서 프로그램이 실행될때 동작합니다.
        /// </summary>
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddCommonServices();
            services.AddViewsAndViewModels();
            return services;
        }

        /// <summary>
        /// 앱 전반에서 사용되는 공통 서비스를 등록합니다.
        /// </summary>
        private static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IIdentityService, IdentityService>();
            services.AddSingleton<IWindowControlService, WindowControlService>();

            // request 요청마다 AuthHeaderHandler가 생성되야하므로 Transient로 설정
            services.AddTransient<AuthHeaderHandler>();

            services.AddBaseSettingHttpClient<IAuthService, AuthService>();
            // Request를 전송할때 Header에 Token을 삽입해야하므로 AddHandler를 적용해줌
            services.AddBaseSettingHttpClient<IFriendService, FriendService>().AddHttpMessageHandler<AuthHeaderHandler>();
            return services;
        }

        /// <summary>
        /// 리플렉션을 이용해 모든 ViewModel들과 Window 생성이 필요한 View를 자동으로 등록합니다.
        /// </summary>
        private static IServiceCollection AddViewsAndViewModels(this IServiceCollection services)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // 1. BaseViewModel을 상속받는 클래스 자동 등록
            IEnumerable<Type> viewModels = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseViewModel)));
            foreach (Type vm in viewModels)
            {
                services.AddTransient(vm);
            }
            // MainWindowViewModel은 프로그램의 뿌리가 되는 ViewModel이여서 싱글톤으로 재정의하여 관리
            services.AddSingleton<MainWindowViewModel>();

            // 2. Window를 상속받는 클래스 자동 등록
            IEnumerable<Type> views = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Window)));
            foreach (Type view in views)
            {
                services.AddTransient(view);
            }

            return services;
        }

        /// <summary>
        /// 서버 주소 등 공통 HttpClient 설정을 적용하는 확장 메서드
        /// </summary>
        private static IHttpClientBuilder AddBaseSettingHttpClient<TClient, TImplementation>(this IServiceCollection services)
            where TClient : class
            where TImplementation : class, TClient
        {
            return services.AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5204");
            });
        }
    }
}
