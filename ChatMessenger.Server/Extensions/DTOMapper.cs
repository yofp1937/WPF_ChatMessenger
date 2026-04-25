/*
 * 엔티티(Entity) 모델을 클라이언트 전송용 DTO로 변환하는 확장 메서드들을 정의하는 클래스입니다.
 */
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Shared.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Extensions
{
    public static class DTOMapper
    {
        /// <summary>
        /// IQueryable(DB 쿼리) 단계에서 Friendship과 User 테이블을 Join하여 필요한 컬럼만 선별적으로 추출(Projection)합니다.
        /// </summary>
        /// <remarks>
        /// Db 엔티티 전체를 서버 메모리에 로드한 뒤 변환하면 불필요한 Password같은 User 데이터도 읽어와야하는데,<br/>
        /// 해당 메서드를 사용하면 FriendResponse에 구조에 맞는 SELECT 쿼리를 생성해줍니다.<br/>
        /// 이로써 네트워크 트래픽이 감소하고, 서버 메모리 사용량이 최소화됩니다.
        /// </remarks>
        /// <param name="friendships">내 친구관계 설계도</param>
        /// <param name="users">User 테이블 포인터</param>
        /// <returns>최적화된 Sql 쿼리 설계도가 담긴 IQueryable 객체</returns>
        public static IQueryable<FriendResponse> ProjectToFriendResponse(this IQueryable<Friendship> friendships, DbSet<User> users)
        {
            return friendships.Join(users,                              // 1.friendships 테이블에 Join을 걸어서 users 테이블과 합쳐서 데이터를 추출하겠다.
                friendship => friendship.FriendEmail,               // 2.friendship의 FriendEmail과 user의 Email이 일치하는 데이터를 찾겠다.
                user => user.Email,
                (friendship, user) => new FriendResponse        // 3.두 테이블의 데이터를 토대로 FriendResponse 객체를 만들겠다.
                {                                                             // (Db는 FriendResponse가 뭔지 모르고 중괄호 안에 선언된 데이터만 추출해서 전송하고,
                    Email = user.Email,                                  // FriendResponse 객체로 조립하는건 Server에서 실행됨)
                    Nickname = user.Nickname,
                    StatusMessage = user.StatusMessage,
                    ProfileImageURL = user.ProfileImageURL,     // 4.여기까진 User 테이블에서 데이터 추출
                    IsAdded = true,                                      // (데이터를 추출한다는건 친구 추가 상태임을 의미하니 true로 설정)
                    IsBlocked = friendship.IsBlocked,
                    IsFavorite = friendship.IsFavorite,                // 5.여기까진 Friendship 테이블에서 데이터 추출
                    IsMe = false                                          // (나와 친구 등록된 데이터를 추출하는거라 IsMe는 false) 
                });
        }

        /// <summary>
        /// User Entity를 기반으로 FriendResponse DTO를 생성합니다.
        /// </summary>
        /// <param name="user">변환할 원본 User 객체</param>
        /// <param name="friendship">나와의 관계 정보(선택 사항)</param>
        // 매개변수 user에 this를 붙임으로써 User의 내부 메서드처럼 호출이 가능해짐
        public static FriendResponse MapToFriendResponse(this User user, Friendship? friendship = null, bool isMe = false)
        {
            return new FriendResponse
            {
                Email = user.Email,
                Nickname = user.Nickname,
                StatusMessage = user.StatusMessage,
                ProfileImageURL = user.ProfileImageURL,

                IsMe = isMe,
                IsAdded = friendship != null && !friendship.IsBlocked,
                IsBlocked = friendship?.IsBlocked ?? false,
                IsFavorite = friendship?.IsFavorite ?? false,
            };
        }
    }
}
