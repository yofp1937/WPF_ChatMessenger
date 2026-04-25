using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Messages
{
    /// <summary>
    /// 친구 추가가 성공적으로 완료됐을때 다른 ViewModel에게 알려주기위한 메세지
    /// </summary>
    public record FriendAddedMessage(FriendModel friend);
}
