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
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatParticipant> ChatParticipants { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

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
            OnChatRoomModelCreating(modelBuilder);
            OnChatParticipantModelCreating(modelBuilder);
            OnChatMessageModelCreating(modelBuilder);
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
                // 주체(나)와의 관계
                entity.HasOne(f => f.User)                      // Friendship은 하나의 User를 갖는다.
                       .WithMany()                                 // User는 여러개의 Friendship을 가질 수 있다.
                       .HasForeignKey(f => f.UserEmail)      // Friendship의 UserEmail을 외래 키로 등록한다.
                       .OnDelete(DeleteBehavior.Cascade);   // User가 삭제되면 Friendship도 전부 삭제된다.

                // 대상(친구)과의 관계
                entity.HasOne(f => f.Friend)
                      .WithMany()
                      .HasForeignKey(f => f.FriendEmail)
                      .OnDelete(DeleteBehavior.NoAction); // 다중 Cascade 경로 충돌 방지를 위해 NoAction 설정
                // TODO: User가 탈퇴할경우 Friendship을 정리해주는 로직을 작성해줘야함

                // 중복 친구 추가를 방지하는 인덱스 설정
                entity.HasIndex(f => new { f.UserEmail, f.FriendEmail }).IsUnique();
            });
        }
        /// <summary>
        /// ChatRoom의 제약조건을 설정합니다.
        /// </summary>
        private void OnChatRoomModelCreating(ModelBuilder modelBuilder)
        {
            /* ChatRoom의 Id 컬럼에 대해 NEWSEQUENTIALID() 함수를 기본값으로 사용하도록 설정
             *
             * [NEWSEQUENTIALID()를 왜 설정했는지에대한 설명]
             * MSSQL은 데이터를 PK값의 순서대로 물리적인 저장소에 저장하는 특성을 가졌다.
             * Guid를 PK로 설정하고 랜덤으로 생성하면 새로운 데이터를 저장할때,
             * 기존 데이터들을 한칸씩 전부 뒤로 밀어내고 그 사이에 데이터를 저장해야한다.
             * 신규 Guid를 중간에 끼워 맞추기위해 기존 데이터를 밀어내고 자리를 찾는 과정이 반복되다보면 성능은 반드시 떨어진다.
             * 
             * 이와 동시에 일어나는게 페이지 분할이다.
             * DB는 데이터를 Page라는(보통 8KB) 단위로 관리한다.
             * 이미 꽉찬 페이지에 신규 Guid 데이터가 들어와야하면 DB는 새로운 페이지를 만들고 기존 페이지를 절반으로 나눈다.
             * 그럼 순차적으로 저장돼있던 데이터들이 A~K까지는 1페이지에 들어있고 L~Z까지는 300페이지에 위치하게되면서
             * DB에서 데이터를 순서대로 읽으려할때 1페이지를 읽고 2페이지를 읽는게 아니라 300번까지 갔다가 다시 2페이지로 돌아오는 현상이 발생하게된다.
             * 
             * 이를 방지하는게 NEWSEQUENTIALID()인데 이건 값을 생성할때 항상 이전 값보다 큰 값을 생성하게하는 함수이다.
             * 항상 이전보다 큰 값을 생성하므로 데이터가 생성되면 중간을 비집고 들어가지않고 마지막 페이지의 끝에 데이터를 붙이게된다.
             */
            modelBuilder.Entity<ChatRoom>()
                .Property(x => x.Id)
                .HasDefaultValueSql("NEWSEQUENTIALID()");
        }
        /// <summary>
        /// ChatParticipant의 제약조건을 설정합니다.
        /// </summary>
        private void OnChatParticipantModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatParticipant>(entity =>
            {
                // 유저와의 관계
                entity.HasOne(p => p.User)
                      .WithMany()
                      .HasForeignKey(p => p.UserEmail)
                      .OnDelete(DeleteBehavior.Cascade); // 유저 탈퇴 시 참여 정보 삭제

                // 채팅방과의 관계
                entity.HasOne(p => p.ChatRoom)
                      .WithMany()
                      .HasForeignKey(p => p.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade); // 방이 삭제되면 참여 정보도 삭제
            });
        }
        /// <summary>
        /// ChatMessage의 제약조건을 설정합니다.
        /// </summary>
        private void OnChatMessageModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                // 유저와의 관계
                entity.HasOne(m => m.Sender)
                      .WithMany()
                      .HasForeignKey(m => m.SenderEmail)
                      .OnDelete(DeleteBehavior.NoAction); // 유저가 탈퇴해도 메세지는 남겨야함

                // 채팅방과의 관계
                entity.HasOne(m => m.ChatRoom)
                      .WithMany()
                      .HasForeignKey(m => m.ChatRoomId)
                      .OnDelete(DeleteBehavior.Cascade); // 방이 삭제되면 메시지 전부 삭제
                
                // 데이터 검색시 방 식별번호로 필터링하고, 그 안에서 보낸 시간 순서로 인덱스를 정렬함
                entity.HasIndex(m => new { m.ChatRoomId, m.SentAt });
            });
        }
        #endregion
    }
}
