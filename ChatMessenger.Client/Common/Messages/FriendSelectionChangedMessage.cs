/*
 * Messenger를 통해 FriendList에서 FriendDetail로 데이터를 전송할때 사용하는 메세지
 */
using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Messages
{
    /// <summary>
    /// FriendList에서 선택된 친구가 변경됐을때 전송하는 메세지
    /// </summary>
    public record FriendSelectionChangedMessage(FriendModel friend);
}
