/*
 * DB 테이블과 매핑되는 1:1 엔티티 클래스
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatMessenger.Shared.Models
{
    public class User
    {
        // User가 로그인할 때 사용해야할 Id (Primary Key)
        [Key] // 속성을 Primary Key로 지정하겠다는 의미
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Db가 자동으로 값을 생성하게 명령
        public Guid Id { get; set; }
        // User가 로그인할 때 사용해야하는 Email
        [EmailAddress] // 값이 이메일 형식인지 체크
        public string Email { get; set; } = string.Empty;
        // User가 로그인할 때 사용해야하는 Password
        // TODO: 나중에 암호화 해야함
        public string Password { get; set; } = string.Empty;
        // User의 별명
        public string NickName { get; set; } = string.Empty;
    }
}
