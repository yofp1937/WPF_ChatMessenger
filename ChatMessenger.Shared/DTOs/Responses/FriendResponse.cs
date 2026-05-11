/*
 * Server에서 Client에게 친구의 정보를 전송하기위한 DTO
 */
namespace ChatMessenger.Shared.DTOs.Responses
{
    /// <summary>
    /// Server에서 Clinet 측으로 한 유저의 정보를 전송하는 DTO입니다.
    /// </summary>
    /// <remarks>
    /// 로그인한 사용자와의 관계 정보도 포함할 수 있습니다.
    /// </remarks>
    public class FriendResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string? ProfileImageURL { get; set; }

        public bool IsMe { get; set; }
        public bool IsAdded { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsFavorite { get; set; }
    }
}