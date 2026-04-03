/*
 * Messenger를 통해 MainWindowViewModel에게 화면 전환을 요청할때 사용하는 메세지
 */
namespace ChatMessenger.Client.Common.Messages
{
    /// <summary>
    /// 하위 ViewModel에서 Messenger를 통해 MainWindowViewModel에게 화면 전환을 요청할때 사용하는 Message
    /// </summary>
    /// <param name="ViewModelType">typeof()를 사용하여 전환을 원하는 View의 ViewModel 입력</param>
    public record NavigationMessage(Type ViewModelType);
}
