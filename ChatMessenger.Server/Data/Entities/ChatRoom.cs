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
        // 1대1 채팅의 경우 값이 null로 들어갈 예정이고, ViewModel에서 상대방 닉네임을 띄워줘야함
        [MaxLength(24, ErrorMessage = "채팅방 이름은 24자 이내로 입력해주세요.")]
        public string? Title { get; set; }
        public string? RoomProfileImageURL { get; set; }

        // 채팅방 생성 날짜
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // 1대1 채팅인지, 그룹 채팅인지 여부
        [Required]
        public bool IsGroupChat { get; set; }
    }
}
