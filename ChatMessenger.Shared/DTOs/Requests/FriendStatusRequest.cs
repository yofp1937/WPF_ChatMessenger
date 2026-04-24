namespace ChatMessenger.Shared.DTOs.Requests
{
    /// <summary>
    /// 친구의 즐겨찾기, 차단 상태를 변경하기위한 요청을 전달할때 사용하는 DTO입니다.
    /// </summary>
    public class FriendStatusRequest
    {
        public string Email { get; set; } = string.Empty!;
        public bool IsFavorite { get; set; }
        public bool IsBlocked { get; set; }
    }
}
