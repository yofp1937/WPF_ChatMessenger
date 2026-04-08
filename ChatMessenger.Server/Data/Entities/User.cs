/*
 * 사용자의 정보를 저장하는 Table
 */
using System.ComponentModel.DataAnnotations;

namespace ChatMessenger.Server.Data.Entities
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
        [MaxLength(16)]
        public string NickName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string StatusMessage { get; set; } = string.Empty;

        [Required]
        public string ProfileImageURL { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
