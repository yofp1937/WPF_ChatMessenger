/*
 * Server에서 Client에게 친구의 정보를 전송하기위한 DTO
 */
namespace ChatMessenger.Shared.DTOs.Responses
{
    public class FriendResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string ProfileImageURL { get; set; } = string.Empty;
    }
}