using ChatMessenger.Server.Data;
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces.Friend;
using ChatMessenger.Server.Mappers;
using ChatMessenger.Shared.DTOs.Requests;
using ChatMessenger.Shared.DTOs.Responses.Friend;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Services.Friend
{
    /// <summary>
    /// FriendController의 요청에따라 친구와 관련된 요청(검색, 추가, 상태 변경 등)을 처리해주는 Service입니다.
    /// </summary>
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _context;

        public FriendService(AppDbContext context)
        {
            _context = context;
        }

        #region public Method
        /// <inheritdoc/>
        public async Task<List<FriendResponse>> GetFriendListAsync(string myEmail)
        {
            return await _context.Friendships
                // 1. Friendship 테이블에서 기본적으로 아래 조건을 충족하는 데이터를 사용
                .Where(f => f.UserEmail == myEmail && !f.IsBlocked)
                // 2. User 테이블과 Join하여 FriendResponse 구조로 데이터를 추출할 Sql 쿼리 생성
                .ProjectToFriendResponse(_context.Users)
                // 3. Sql을 Db에서 실행하고 결과 데이터를 FriendResponse 객체 리스트로 반환
                .ToListAsync();
        }
        /// <inheritdoc/>
        public async Task<FriendResponse?> SearchUserAsync(string myEmail, string friendEmail)
        {
            // 1. 검색하려는 유저가 존재하는지 확인
            User? targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == friendEmail);
            if (targetUser == null) return null;
            // 2. 존재하면 친구 관계 확인
            Friendship? friendship = await GetFriendshipAsync(myEmail, friendEmail);
            // 3. 유저와의 친구 관계 정보를 FriendResponse로 Mapping하여 반환
            return targetUser.MapToFriendResponse(friendship, myEmail == friendEmail);
        }
        /// <inheritdoc/>
        public async Task<FriendResponse?> AddFriendAsync(string myEmail, string friendEmail)
        {
            // 1. 이미 등록된 친구인지 확인
            Friendship? friendship = await GetFriendshipAsync(myEmail, friendEmail);
            if (friendship != null) return null;
            try
            {
                // 2. 친구 추가 진행 (Db에 데이터 삽입)
                await AddFriendshipAsync(myEmail, friendEmail);
                // 3. 친구 추가 성공했으면 SearchUserAsync 사용하여 해당 User의 Profile 반환
                return await SearchUserAsync(myEmail, friendEmail);
            }
            catch (Exception ex)
            {
                // 4. Db 저장 중 오류 발생 시 처리
                // TODO: 나중에 실패 이유를 로그에 저장
                Console.WriteLine($"[{nameof(FriendService)}_{nameof(AddFriendAsync)}]: {ex.Message}");
                return null;
            }
        }
        /// <inheritdoc/>
        public async Task<bool> DeleteFriendAsync(string myEmail, string friendEmail)
        {
            // 1. 등록된 친구가 맞는지 확인
            Friendship? friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserEmail == myEmail && f.FriendEmail == friendEmail);
            if (friendship == null) return false;
            // 2. 친구 삭제 성공했으면 return
            _context.Friendships.Remove(friendship);
            return await _context.SaveChangesAsync() > 0;
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateFavoriteAsync(string myEmail, FriendStatusRequest request)
        {
            // 1. 등록된 친구가 맞는지 확인
            Friendship? friendship = await GetFriendshipAsync(myEmail, request.Email);
            if (friendship == null) return false;
            // 2. 즐겨찾기 변경 성공했으면 return
            friendship.IsFavorite = request.IsFavorite;
            return await _context.SaveChangesAsync() > 0;
        }
        /// <inheritdoc/>
        public async Task<bool> UpdateBlockAsync(string myEmail, FriendStatusRequest request)
        {
            // 1. 등록된 친구가 맞는지 확인
            Friendship? friendship = await GetFriendshipAsync(myEmail, request.Email);
            // 2. 대상을 차단하려하는 경우
            if (request.IsBlocked)
            {
                // 2-1. 모르는 User를 차단하면 새로운 관계 생성
                if (friendship == null)
                {
                    _context.Friendships.Add(new Friendship { UserEmail = myEmail, FriendEmail = request.Email, IsBlocked = true });
                }
                else // 2-2. 친구를 차단하면 값 변경
                {
                    friendship.IsBlocked = true;
                    friendship.IsFavorite = false;
                }
            }
            // 3. 차단 해제의 경우
            else
            {
                // 3-1. 친구 관계가 없으면 오류
                if (friendship == null) return false;
                // 3-2. 친구 관계를 아예 제거
                _context.Friendships.Remove(friendship);
            }
            // 4. 성공했으면 return
            return await _context.SaveChangesAsync() > 0;
        }
        #endregion public Method
        #region private Method
        /// <summary>
        /// 해당 유저와의 친구 관계 정보를 가져옵니다.
        /// </summary>
        /// <param name="myEmail">친구 관계 추출에 필요한 로그인한 User의 Email</param>
        /// <param name="friendEmail">친구 관계 추출 대상 User의 Email</param>
        /// <returns>해당 User와의 친구 관계</returns>
        private async Task<Friendship?> GetFriendshipAsync(string myEmail, string friendEmail)
        {
            return await _context.Friendships.FirstOrDefaultAsync(f => f.UserEmail == myEmail && f.FriendEmail == friendEmail);
        }
        private async Task AddFriendshipAsync(string myEmail, string friendEmail)
        {
            // 1. 나와 대상 User와의 새로운 친구 관계 생성
            Friendship newFriendship = new Friendship
            {
                UserEmail = myEmail,
                FriendEmail = friendEmail,
                IsFavorite = false,
                IsBlocked = false,
            };
            // 2. Db에 저장
            _context.Friendships.Add(newFriendship);
            await _context.SaveChangesAsync();
        }
        #endregion private Method
    }
}
