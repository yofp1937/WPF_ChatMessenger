using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Server.Services.Bases;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Repositories
{
    public class FriendshipRepositoryService : BaseRepositoryService, IFriendshipRepository
    {
        public FriendshipRepositoryService(AppDbContext context) : base(context) { }

        /// <inheritdoc/>
        public async Task<List<FriendResponse>> GetFriendResponseListAsync(string userEmail)
        {
            return await ExecuteDbActionAsync(() =>
                _context.Friendships
                // 1. Friendship 테이블에서 기본적으로 아래 조건을 충족하는 데이터를 사용
                .Where(f => f.UserEmail == userEmail && !f.IsBlocked)
                // 2. User 테이블과 Join하여 FriendResponse 구조로 데이터를 추출할 Sql 쿼리 생성
                .ProjectToFriendResponse(_context.Users)
                // 3. Sql을 Db에서 실행하고 결과 데이터를 FriendResponse 객체 리스트로 반환
                .ToListAsync()
                );
        }
        /// <inheritdoc/>
        public async Task<Friendship?> GetFriendshipEntityAsync(string userEmail, string friendEmail)
        {
            return await _context.Friendships
                .Include(f => f.Friend)
                .FirstOrDefaultAsync(f => f.UserEmail == userEmail && f.FriendEmail == friendEmail);
        }
        /// <inheritdoc/>
        public async Task<bool> AddFriendshipEntityAsync(string userEmail, string friendEmail, bool isFavorite = false, bool isBlocked = false)
        {
            // 1. 대상 User와의 새로운 친구 관계 생성
            Friendship newFriendship = new Friendship
            {
                UserEmail = userEmail,
                FriendEmail = friendEmail,
                IsFavorite = isFavorite,
                IsBlocked = isBlocked,
            };
            // 2. Db에 저장
            _context.Friendships.Add(newFriendship);
            return await _context.SaveChangesAsync() > 0;
        }
        /// <inheritdoc/>
        public async Task<bool> RemoveFriendshipEntityAsync(Friendship friendship)
        {
            _context.Remove(friendship);
            return await _context.SaveChangesAsync() > 0;
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateFriendshipAsync(Friendship friendship, Action<Friendship> updateAction)
        {
            return await ExecuteDbActionAsync(async () =>
            {
                // 1. 외부에서 전달된 updateAction 실행
                updateAction(friendship);

                // 2. Db 저장 시도 후 결과 값 반환
                return await _context.SaveChangesAsync() > 0;
            });
        }
    }
}
