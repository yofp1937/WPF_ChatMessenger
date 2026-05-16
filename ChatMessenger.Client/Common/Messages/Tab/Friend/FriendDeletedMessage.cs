using ChatMessenger.Client.Models.Friends;

namespace ChatMessenger.Client.Common.Messages.Tab.Friend
{
    /// <summary>
    /// 친구 삭제가 성공적으로 완료됐을때 다른 ViewModel에게 알려주기위한 메세지
    /// </summary>
    public record FriendDeletedMessage(FriendModel friend);
}
