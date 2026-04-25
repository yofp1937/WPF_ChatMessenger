/*
 * 채팅방의 정보를 저장하는 테이블 구조입니다.
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMessenger.Server.Data.Entities
{
    public class ChatRoom
    {
        // 채팅방 식별 Id
        [Key]
        // Database에서 Guid 생성을 담당하도록 설정 (AppDbContext에 자세한 설명 작성)
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // 채팅방 이름
        [Required]
        [MaxLength(24)]
        public string? Title { get; set; }

        // 채팅방 생성 날짜
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 1대1 채팅인지, 그룹 채팅인지 여부
        [Required]
        public bool IsGroupChat { get; set; }
    }
}
