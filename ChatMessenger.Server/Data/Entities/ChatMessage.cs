/*
 * 유저들이 주고받은 대화 내용을 저장하는 테이블의 구조입니다.
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMessenger.Server.Data.Entities
{
    public class ChatMessage
    {
        // 메세지 식별 번호 (데이터가 가장 많이 쌓이는곳이라 long 사용)
        [Key]
        // 데이터 생성시 값이 1씩 증가하게 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        // 1.실제로 DB 컬럼에 저장될 값 (채팅방 식별 번호)
        [Required]
        public Guid ChatRoomId { get; set; }
        // 2.ChatRoomId에 데이터를 가져올 객체
        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        // 1.실제로 DB 컬럼에 저장될 값 (채팅 전송자)
        [Required]
        public string SenderEmail { get; set; } = string.Empty;
        // 2.SenderEmail에 데이터를 가져올 객체
        [ForeignKey("SenderEmail")]
        public virtual User Sender { get; set; } = null!;

        // 채팅 내용
        [Required]
        public string Content { get; set; } = string.Empty;

        // 채팅 보낸 시간
        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
