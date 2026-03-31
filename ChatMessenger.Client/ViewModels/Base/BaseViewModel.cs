/*
 * 모든 ViewModel들의 조상이되는 클래스
 * 다형성 활용, 추후 공통 기능 구현 등을 위해 생성
 */
using CommunityToolkit.Mvvm.ComponentModel;

namespace ChatMessenger.Client.ViewModels.Base
{
    /// <summary>
    /// 모든 ViewModel의 부모 클래스입니다.
    /// </summary>
    public abstract partial class BaseViewModel : ObservableObject
    {

    }
}