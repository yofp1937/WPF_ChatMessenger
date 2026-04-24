/*
 * Client에서 Server에 친구 추가, 삭제를 요청할때 사용하는 DTO
 */
namespace ChatMessenger.Shared.DTOs.Requests
{
    public class AddorDeleteFriendRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
