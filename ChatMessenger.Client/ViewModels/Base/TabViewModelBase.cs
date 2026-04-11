/*
 * MainShellViewModel의 CurrentViewModel에 할당되는 '페이지 내부의 콘텐츠를 담당'하는 ViewModel들의 추상 클래스
 * 
 * 1.한 페이지 내부에서 Navigation Menu에따라 동적으로 변경되는 화면의 데이터 처리를 담당
 * 2.부모 PageViewModel의 수명 주기 내에서 Singleton 혹은 Transient로 관리
 */
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.ViewModels.Base
{
    /// <summary>
    /// 한 화면에서 동적으로 변경되는 화면의 데이터 처리를 담당합니다.
    /// </summary>
    public abstract partial class TabViewModelBase : BaseViewModel
    {
        [ObservableProperty]
        private BaseViewModel? _leftContentViewModel;
        [ObservableProperty]
        private BaseViewModel? _rightContentViewModel;

        public TabViewModelBase() { }
    }
}
