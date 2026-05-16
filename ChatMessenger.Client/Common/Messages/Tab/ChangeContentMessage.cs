using ChatMessenger.Client.Common.Enums;

namespace ChatMessenger.Client.Common.Messages.Tab
{
    /// <summary>
    /// ContentPanelVM에서 CurrentVM을 변경할때 사용하는 메세지입니다.
    /// </summary>
    /// <remarks>
    /// 하위 VM에서 ContentPanelVM의 CurrentVM을 변경해야할때 사용합니다.
    /// </remarks>
    /// <param name="type">교체를 원하는 화면 종류</param>
    public record ChangeContentMessage(ContentPanelType type);
}
