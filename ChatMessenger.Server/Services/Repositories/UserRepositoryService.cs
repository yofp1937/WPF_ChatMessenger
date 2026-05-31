using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Repositories
{
    public class UserRepositoryService : BaseRepositoryService, IUserRepositoryService
    {
        public UserRepositoryService(AppDbContext context) : base(context) { }

        #region public Method
        /// <inheritdoc/>
        public async Task<bool> FindUserByEmailAsync(string email)
        {
            return await ExecuteDbActionAsync(() =>
                _context.Users.AnyAsync(u => u.Email == email));
        }
        /// <inheritdoc/>
        public async Task<bool> AddNewUserAsync(string email, string password, string nickname)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. 새로운 User Entity 생성
                User newUser = new()
                {
                    Email = email,
                    Password = password,
                    Nickname = nickname
                };
                // 2. 등록 대기 및 실제 등록
                _context.Users.Add(newUser);
                return await _context.SaveChangesAsync() > 0;
            });
        }
        /// <inheritdoc/>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await ExecuteDbActionAsync(() =>
                _context.Users.FirstOrDefaultAsync(u => u.Email == email));
        }
        /// <inheritdoc/>
        public async Task<List<string>> GetNicknamesByEmailsAsync(IEnumerable<string> emails)
        {
            return await ExecuteDbActionAsync(() =>
                _context.Users
                    .AsNoTracking()
                    .Where(u => emails.Contains(u.Email))
                    .Select(u => u.Nickname)
                    .ToListAsync()
            );
        }
        #endregion public Method
    }
}
