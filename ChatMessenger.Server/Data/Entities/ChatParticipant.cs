/*
 * 채팅방의 참가자 정보를 저장하는 테이블 구조입니다.
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMessenger.Server.Data.Entities
{
    public class ChatParticipant
    {
        // 채팅방 참여자 데이터 식별번호 (참여자 데이터는 ChatMessage에비해 많지않기때문에 int로 설정)
        [Key]
        // 데이터 생성시 값이 1씩 증가하게 설정
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // 1.실제로 DB 컬럼에 저장될 값 (채팅방 식별 번호)
        [Required]
        public Guid ChatRoomId { get; set; }
        // 2.ChatRoomId에 데이터를 가져올 객체
        [ForeignKey("ChatRoomId")]
        public virtual ChatRoom ChatRoom { get; set; } = null!;

        // 1.실제로 DB 컬럼에 저장될 값 (채팅방 참가자)
        [Required]
        public string UserEmail { get; set; } = string.Empty;
        // 2.UserEmail에 데이터를 가져올 객체
        [ForeignKey("UserEmail")]
        public virtual User User { get; set; } = null!;

        // 이 유저가 마지막으로 읽은 메세지 식별 번호
        public long LastReadMessageId { get; set; }
    }
}
