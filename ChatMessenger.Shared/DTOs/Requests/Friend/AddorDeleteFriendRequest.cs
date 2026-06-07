/*
 * Client에서 Server에 친구 추가, 삭제를 요청할때 사용하는 DTO
 */
using ChatMessenger.Shared.DTOs.Requests.Base;

namespace ChatMessenger.Shared.DTOs.Requests.Friend
{
    public class AddorDeleteFriendRequest : BaseRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
