using System.Windows;
using ChatMessenger.Client.Common.Interfaces;
using ChatMessenger.Client.Configs;
using ChatMessenger.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ChatMessenger.Client
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public App()
        {
            ServiceCollection services = new();
            // "/Config/DependencyInjectionConfig"에 선언된 AddAppServices 메서드 실행
            services.AddAppServices();
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// MainWindow.xaml의 폴더 위치를 바꿔도 문제없이 실행하기위해
        /// 프로젝트 실행시 Application_Startup을 호출하도록 변경
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            IWindowService windowService = ServiceProvider.GetRequiredService<IWindowService>();
            MainWindowViewModel mainViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
            windowService.ShowWindow(mainViewModel);
        }
    }
}
