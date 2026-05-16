using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Client.Common.Messages.Tab.Friend
{
    /// <summary>
    /// 친구 추가 패널을 열리게 하거나 닫게하기위해 전달하는 메세지입니다.
    /// </summary>
    /// <remarks>
    /// ContentPanelViewModel에서 수신하여 Panel을 열고 닫습니다.
    /// </remarks>
    public record AddFriendModeChangedMessage();
}
