using System.Configuration;
using System.Data;
using System.Windows;

namespace ChatMessenger.Client
{
    public partial class App : Application
    {
        /// <summary>
        /// MainWindow.xaml의 폴더 위치를 바꿔도 문제없이 실행하기위해
        /// 프로젝트 실행시 Application_Startup을 호출하도록 변경
        /// </summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
