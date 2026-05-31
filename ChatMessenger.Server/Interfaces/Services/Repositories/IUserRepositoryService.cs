using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.Common;

namespace ChatMessenger.Server.Interfaces.Services.Repositories
{
    public interface IUserRepositoryService : IBaseRepositoryService
    {
        /// <summary>
        /// Email로 User가 존재하는지 확인합니다.
        /// </summary>
        /// <param name="email">찾으려는 User의 Email</param>
        /// <returns>존재하면 true, 없으면 false</returns>
        Task<bool> FindUserByEmailAsync(string email);
        /// <summary>
        /// User Table에 새로운 Entity를 추가합니다.
        /// </summary>
        /// <param name="email">등록하려는 User의 Email</param>
        /// <param name="password">등록하려는 User의 Password</param>
        /// <param name="nickname">등록하려는 User의 Nickname</param>
        /// <returns>등록 성공시 true, 실패시 false</returns>
        Task<bool> AddNewUserAsync(string email, string password, string nickname);
        /// <summary>
        /// Email로 User Entity를 찾습니다.
        /// </summary>
        /// <param name="email">찾으려는 User의 Email</param>
        /// <returns>해당 Email을 사용하는 User Entity, 없으면 null</returns>
        Task<User?> GetUserByEmailAsync(string email);
        /// <summary>
        /// Email로 User를 찾아서 Nickname을 반환받습니다.
        /// </summary>
        /// <param name="emails">Nickname을 반환받으려는 User들의 Email List</param>
        /// <returns>Email을 사용하는 User들의 Nickname List</returns>
        Task<List<string>> GetNicknamesByEmailsAsync(IEnumerable<string> emails);
       
    }
}
