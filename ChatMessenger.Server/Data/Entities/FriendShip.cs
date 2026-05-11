/*
 * 사용자들의 친구, 차단, 즐겨찾기 정보를 저장합니다.
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMessenger.Server.Data.Entities
{
    public class Friendship
    {
        [Key]
        // 데이터 생성시 값이 1씩 증가하게 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // 추가한 주체(나)
        [Required]
        public string UserEmail { get; set; } = null!;
        [ForeignKey("UserEmail")]
        public virtual User User { get; set; } = null!;

        // 추가된 상대
        [Required]
        public string FriendEmail { get; set; } = null!;
        [ForeignKey("FriendEmail")]
        public virtual User Friend { get; set; } = null!;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsBlocked { get; set; } // 차단 여부
        public bool IsFavorite { get; set; } // 즐겨찾기 여부
    }
}
