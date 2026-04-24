using ChatMessenger.Shared.DTOs.Responses;

namespace ChatMessenger.Client.Common.Messages
{
    /// <summary>
    /// 친구 삭제가 성공적으로 완료됐을때 다른 ViewModel에게 알려주기위한 메세지
    /// </summary>
    public record FriendDeletedMessage(FriendResponse friend);
}
