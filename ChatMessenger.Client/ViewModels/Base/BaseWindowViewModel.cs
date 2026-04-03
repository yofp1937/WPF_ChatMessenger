/*
 * Window 창을 제어해야하는 ViewModel들의 부모 클래스
 * 공통 창 제어 로직(최소화, 최대화, 닫기)을 포함
 */
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace ChatMessenger.Client.ViewModels.Base
{
    public abstract partial class BaseWindowViewModel : BaseViewModel
    {
        public BaseWindowViewModel() { }

        #region Protected Method
        /* [RelayCommand] 참고 사항
         * View에서 Command로 Binding된 메서드들은 [RelayCommand]를 상단에 붙여서 사용합니다.
         * [RelayCommand]가 붙어있을시 CommunityToolkit.Mvvm의 소스 생성기가 컴파일 타이밍에 ICommand Property를 자동으로 생성해줍니다.
         * 따라서 기존 ICommand와 연결된 Method에 입력해야했던 매개변수 (object sender, EventArgs e)를 없애도 됩니다.
         * Property의 이름은 '{메서드명}Command'로 선언되므로 View에서는 '{메서드명}Command'로 Binding하여 사용합니다. */
        /// <summary>
        /// 창을 최소화합니다.
        /// </summary>
        [RelayCommand]
        protected virtual void MinimizeWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// 창을 최대화합니다.
        /// </summary>
        [RelayCommand]
        protected virtual void MaximizeWindow()
        {
            Window window = Application.Current.MainWindow;
            window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }
        /// <summary>
        /// 창을 닫습니다.
        /// <br/>(TODO: 나중에 설정에 창 닫을때 시스템 트레이로 이동할지, 프로그램 종료할지 결정하는거 넣기)
        /// </summary>
        [RelayCommand]
        protected virtual void CloseWindow()
        {
            Application.Current.MainWindow.Close();
        }
        #endregion
    }
}
