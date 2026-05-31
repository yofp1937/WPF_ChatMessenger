using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Client.Common.Messages.Tab.Chat.Room
{
    /// <summary>
    /// 특정 채팅방을 나가서 ChatRoomListViewModel을 갱신해야할때 전송하는 메세지입니다.
    /// </summary>
    /// <param name="roomId">채팅방 식별 번호</param>
    public record LeaveChatRoomMessage(Guid roomId);
}
