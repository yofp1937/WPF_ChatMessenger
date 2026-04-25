using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Messages
{
    /// <summary>
    /// 친구의 상태(즐겨찾기 등)가 변경됐음을 알리는 메세지
    /// </summary>
    public record FriendStatusChangeMessage(FriendModel friend);
}
