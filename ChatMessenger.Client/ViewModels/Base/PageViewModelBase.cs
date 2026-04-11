/*
 * MainWindow의 CurrentViewModel에 할당되는 '최상위 페이지' ViewModel들의 추상 클래스
 * 
 * 1.프로그램의 전체 레이아웃을 완전히 담당
 * 2.MainWindowViewModel의 CurrentViewModel에 Binding되어 현재 상태에따라 프로그램의 메인 화면을 변경
 */
namespace ChatMessenger.Client.ViewModels.Base
{
    /// <summary>
    /// /// 애플리케이션의 대단위 화면 전환을 위한 기반 뷰모델 클래스입니다.
    /// </summary>
    public abstract class PageViewModelBase : BaseViewModel
    {
    }
}
