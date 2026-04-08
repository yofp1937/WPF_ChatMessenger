/*
 * 사용자들의 친구, 차단, 즐겨찾기 정보를 저장합니다.
 */
using System.ComponentModel.DataAnnotations;

namespace ChatMessenger.Server.Data.Entities
{
    public class Friendship
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = null!; // 추가한 주체 (나)

        [Required]
        public string FriendEmail { get; set; } = null!; // 추가된 대상 (상대)

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; }  // 차단 여부
        public bool IsFavorite { get; set; } // 즐겨찾기 여부
    }
}
