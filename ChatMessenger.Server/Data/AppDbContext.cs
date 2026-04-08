/*
 * 데이터 베이스와 Server간의 연결 통로이며
 * /Data/Entities의 클래스들을 바탕으로 DB 테이블을 생성하는 역할을 담당합니다.
 */
using ChatMessenger.Server.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Data
{
    public class AppDbContext : DbContext
    {
        // SQL Server에서 실제 User 테이블이 되는 Property
        public DbSet<User> Users { get; set; }
        public DbSet<Friendship> Friendships { get; set; }

        /// <summary>
        /// 외부에서 설정한 DB 연결 정보를 생성자로 전달받습니다.<br/>
        /// (ServiceProvider가 매개변수 알아서 생성해서 넘겨줌)
        /// </summary>
        /// <param name="options">DB 연결 정보</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // 테이블 이름이나 제약조건을 세밀하게 조정하고 싶을 때 사용하는 메서드
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            OnFriendshipModelCreating(modelBuilder);
        }
        #region private Method
        /// <summary>
        /// Friendship의 제약조건을 설정합니다.
        /// </summary>
        private void OnFriendshipModelCreating(ModelBuilder modelBuilder)
        {
            // Friendship과 User의 관계 설정 (외래 키)
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasOne<User>()                          // Friendship은 하나의 User를 갖는다.
                       .WithMany()                                 // User는 여러개의 Friendship을 가질 수 있다.
                       .HasForeignKey(f => f.UserEmail)      // Friendship의 UserEmail을 외래 키로 등록한다.
                       .OnDelete(DeleteBehavior.Cascade);   // User가 삭제되면 Friendship도 전부 삭제된다.

                // 중복 친구 추가를 방지하는 인덱스 설정
                entity.HasIndex(f => new { f.UserEmail, f.FriendEmail }).IsUnique();
            });
        }
        #endregion
    }
}
