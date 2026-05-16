using ChatMessenger.Client.Common.Enums;

namespace ChatMessenger.Client.Common.Messages.Page
{
    /// <summary>
    /// MainWindowVM의 PageVM을 변경할때 사용하는 메세지입니다.
    /// </summary>
    /// <remarks>
    /// 하위 TabViewModel에서 MainWindow의 화면을 전환시켜야할때 사용합니다.
    /// </remarks>
    /// <param name="type">교체를 원하는 화면 종류</param>
    public record ChangePageMessage(AppPageType type);
}
