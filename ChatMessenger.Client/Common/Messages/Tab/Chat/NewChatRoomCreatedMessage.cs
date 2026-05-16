using ChatMessenger.Client.Models.Chats;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatMessenger.Client.Common.Messages.Tab.Chat
{
    /// <summary>
    /// 새로운 방이 생성됐을때 ChatListViewModel에 추가하기위해 전송하는 메세지
    /// </summary>
    /// <param name="roomModel">생성된 채팅방 정보</param>
    public record NewChatRoomCreatedMessage(ChatRoomSummaryModel roomModel);
}
