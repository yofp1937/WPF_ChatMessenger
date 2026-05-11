/*
 * IDbInitializer를 구현하여
 * MSSQL의 연결과 필수 데이터 초기 생성까지 담당하는 클래스입니다.
 * 
 * 1.외부에서 InitializeDbAsync() 호출하면 일회용 Scope를 생성함
 * 2.Scope 내부에서 Database 없으면 생성하고, AppDbContext에 등록된 Table 확인하고 없으면 생성함
 * 3.Test Data까지 주입하고 Scope 종료함
 */
using ChatMessenger.Server.Data.Entities;
using ChatMessenger.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChatMessenger.Server.Data
{
    public class DbInitializer : IDbInitializer
    {
        #region Property
        private readonly IServiceScopeFactory _scopeFactory;
        #endregion

        #region 생성자
        public DbInitializer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        #endregion

        #region public Method
        /// <inheritdoc/>
        public async Task InitializeDbAsync()
        {
            // 1.DB와 연결 통로 생성
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                Console.WriteLine("[DbInitializer - InitializeDbAsync]: Db를 초기화합니다.");

                // 2.DB 연결 및 생성, 테스트 테이블 입력까지 처리
                if (await ConnectionDbAsync(context))
                {
                    await SeedTestDataAsync(context);
                }
            }
        }
        /// <inheritdoc/>
        public async Task<bool> ConnectionDbAsync(AppDbContext context)
        {
            // DB가 없으면 생성해주는 CheckAndCreateTablesAsync() 호출
            await CheckAndCreateTablesAsync(context);
            try
            {
                Console.WriteLine("[DbInitializer - ConnectionDbAsync]: Db 연결 확인 중...");
                if (await context.Database.CanConnectAsync())
                {
                    string dbName = context.Database.GetDbConnection().Database;
                    Console.WriteLine($"[DbInitializer - ConnectionDbAsync]: MSSQL 서버 {dbName} 데이터베이스에 연결됐습니다.");
                    return true;
                }
                else
                {
                    Console.WriteLine("[DbInitializer - ConnectionDbAsync]: 연결이 거부됐습니다. 서버 설정을 확인하세요.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DbInitializer - ConnectionDbAsync]: 에러 발생 {ex.Message}");
                return false;
            }
        }
        /// <inheritdoc/>
        public async Task CheckAndCreateTablesAsync(AppDbContext context)
        {
            Console.WriteLine("[DbInitializer - CheckAndCreateTablesAsync]: 데이터베이스와 테이블 구조 확인 및 생성 중...");
            /* EnsureCreatedAsync를 사용해서 Database와 Table이 존재하지 않으면 생성
             * 1.context를 생성할때 매개변수로 넣은 option의 database가 존재하는지 확인하고 없으면 생성
             * 2.context 객체 내부에 DbSet으로 선언된 Property들의 정보대로 Table들 존재하는지 확인하고 없으면 생성 */
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("[DbInitializer - CheckAndCreateTablesAsync]: 데이터베이스 구조가 준비되었습니다.");
        }
        /// <inheritdoc/>
        public async Task SeedTestDataAsync(AppDbContext context)
        {
            List<User> userList = await context.Users.ToListAsync();
            Random random = new();
            if (!userList.Any())
            {
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 초기 테스트 유저 데이터 주입 중...");
                string password = "1";

                // 1. User 데이터 시딩
                for (int i = 1; i <= 10; i++)
                {
                    // 홀수면 네이버, 짝수면 지메일
                    string address = (i % 2 == 1) ? "naver.com" : "gmail.com";
                    string nickname = (i % 2 == 1) ? "네이버" : "지메일";

                    User newUser = new()
                    {
                        Email = "test" + i + "@" + address,
                        Password = password,
                        Nickname = "테스터" + i + "_" + nickname
                    };

                    context.Users.Add(newUser);
                    userList.Add(newUser);
                }
                await context.SaveChangesAsync();
                string updateSql = "UPDATE Users SET StatusMessage = N'안녕하세요 ' + Nickname + N'입니다. 잘부탁드립니다.'";

                await context.Database.ExecuteSqlRawAsync(updateSql);
                Console.WriteLine($"[DbInitializer - SeedTestDataAsync]: 테스트 유저 데이터가 생성되었습니다.");
            }

            // 2. 친구 관계(Friendship) 데이터 시딩
            if (!await context.Friendships.AnyAsync())
            {
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 초기 테스트 친구 관계 주입 중...");

                foreach (User user in userList)
                {
                    if (user.Email == userList[0].Email) continue;

                    context.Friendships.Add(new Friendship
                    {
                        UserEmail = userList[0].Email,
                        FriendEmail = user.Email,
                    });
                    context.Friendships.Add(new Friendship
                    {
                        UserEmail = user.Email,
                        FriendEmail = userList[0].Email,
                    });
                }
                await context.SaveChangesAsync();
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 테스트 친구 관계가 생성되었습니다.");
            }

            // 3. 채팅방 데이터 시딩
            if (!await context.ChatRooms.AnyAsync())
            {
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 초기 채팅방 생성 중...");

                // 테스트용 그룹 채팅방 생성
                ChatRoom groupChat = new ChatRoom
                {
                    Title = "테스트용 그룹 채팅방",
                    IsGroupChat = true,
                };
                context.ChatRooms.Add(groupChat);
                await context.SaveChangesAsync();

                foreach (User user in userList)
                {
                    // 0번 유저 그룹 채팅방에 등록하고 그외 반복문은 넘기기
                    if (user.Email == userList[0].Email)
                    {
                        context.ChatParticipants.Add(new ChatParticipant
                        {
                            ChatRoomId = groupChat.Id,
                            UserEmail = userList[0].Email,
                        });
                        continue;
                    }

                    // 유저마다 userList[0]과의 채팅방 생성
                    ChatRoom newRoom = new();
                    context.ChatRooms.Add(newRoom);
                    await context.SaveChangesAsync();

                    // 채팅방 인원 등록
                    context.ChatParticipants.Add(new ChatParticipant
                    {
                        ChatRoomId = newRoom.Id,
                        UserEmail = userList[0].Email,
                    });
                    context.ChatParticipants.Add(new ChatParticipant
                    {
                        ChatRoomId = newRoom.Id,
                        UserEmail = user.Email,
                    });
                    // 채팅방에 메세지 10개 등록 (발신자는 랜덤으로 정함)
                    for (int i = 1; i <= 11; i++)
                    {
                        int ranNum = random.Next(1, 101);
                        // 랜덤 숫자가 홀수면 userList[0]이 보낸 메세지, 짝수면 현재 user가 보낸 메세지
                        string senderEmail = (ranNum % 2 == 1) ? userList[0].Email : user.Email;
                        ChatMessage newMessage = new()
                        {
                            ChatRoomId = newRoom.Id,
                            SenderEmail = senderEmail,
                            Content = $"테스트 메세지 {i}입니다.",
                            SentAt = DateTime.UtcNow.AddMinutes(-(10 - i))
                        };
                        context.ChatMessages.Add(newMessage);
                    }

                    // 그룹 채팅방 참여자로도 등록
                    context.ChatParticipants.Add(new ChatParticipant
                    {
                        ChatRoomId = groupChat.Id,
                        UserEmail = user.Email,
                    });
                    // 그룹 채팅방에도 유저별로 메세지 랜덤하게 등록
                    int messageSendCount = random.Next(0, 5);
                    for (int i = 0; i <= messageSendCount; i++)
                    {
                        int sentAddTime = random.Next(1, 120);
                        ChatMessage newMessage = new()
                        {
                            ChatRoomId = groupChat.Id,
                            SenderEmail = user.Email,
                            Content = $"{user.Nickname}의 그룹 채팅 테스트 메세지 전송입니다.( {i + 1} / {messageSendCount + 1} )",
                            SentAt = DateTime.UtcNow.AddMinutes(-(messageSendCount - i))
                        };
                        context.ChatMessages.Add(newMessage);
                    }
                }
                await context.SaveChangesAsync();
            }

            else
            {
                Console.WriteLine("[DbInitializer - SeedTestDataAsync]: 이미 데이터가 존재하므로 시딩을 건너뜁니다.");
            }
        }
        #endregion
    }
}
