/*
 * DB 테이블과 매핑되는 1:1 엔티티 클래스
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMessenger.Shared.Models
{
    public class User
    {
        [Key] // 속성을 Primary Key로 지정하겠다는 의미
        [Required] // 반드시 입력받아야함을 명시
        [EmailAddress] // 값이 이메일 형식인지 체크
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string NickName { get; set; } = string.Empty;
    }
}
