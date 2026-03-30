/*
 * 데이터 베이스와 Server간의 연결 통로이며
 * ChatMessenger.Shared의 Models\Users.cs를 바탕으로 DB 테이블을 생성하는 설계도 역할을 담당합니다.
 */
using ChatMessenger.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Database
{
    public class AppDbContext : DbContext
    {
        // SQL Server에서 실제 User 테이블이 되는 Property
        public DbSet<User> Users { get; set; }

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
        }
    }
}
