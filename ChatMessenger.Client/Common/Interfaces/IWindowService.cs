/*
 * ViewModel을 전달받으면 해당 ViewModel의 Window를 생성하여 띄워주는 역할을 하는 Class가 구현해야할 Interface
 */
using ChatMessenger.Client.ViewModels.Base;

namespace ChatMessenger.Client.Common.Interfaces
{
    interface IWindowService
    {
        /// <summary>
        /// 넘겨받은 viewModel에 맞는 Window를 생성하여 띄워줍니다.
        /// </summary>
        /// <param name="viewModel">생성하고싶은 Window의 ViewModel</param>
        void ShowWindow(BaseViewModel viewModel);
    }
}
