/*
 * 모든 ViewModel들의 조상이되는 클래스
 * 다형성 활용, 추후 공통 기능 구현 등을 위해 생성
 */
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace ChatMessenger.Client.ViewModels.Base
{
    /// <summary>
    /// 모든 ViewModel의 부모 클래스입니다.
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject
    {
        /// <summary>
        /// 메신저 구독, Action 이벤트를 등록할때 생성자에서 호출하여 사용합니다.
        /// </summary>
        protected virtual void Subscribe() { }
        /// <summary>
        /// ViewModel이 파괴되거나 사용이 중지될때 자원을 정리합니다.<br/>
        /// 자식 클래스에서 override할땐 반드시 base.CleanUp()을 호출해야합니다.
        /// </summary>
        public virtual void CleanUp()
        {
            // 1. 인스턴스에 등록된 모든 메신저 구독을 해제
            WeakReferenceMessenger.Default.UnregisterAll(this);
        }
    }
}