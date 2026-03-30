using ChatMessenger.Server.Database;
using ChatMessenger.Server.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 서비스 등록
builder.Services.AddControllers();
builder.Services.AddOpenApi();

/* 데이터베이스 연결 주소 등록
 * AppDbContext와 IDbInitializer는 반드시 Scope가 있어야만 생성이 가능함 */
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
// 누군가가 IDbInitializer를 요청하면 DbInitializer를 생성해서 전달하게 설정
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

var app = builder.Build();

/* 서버 시작시 DB 초기화 로직 실행
 * using을 사용해서 scope를 잠깐 생성하여 사용한 후 내부 로직이 종료되면 메모리 해제 */
using (IServiceScope scope = app.Services.CreateScope())
{
    /* ServiceProvider = 해당 프로그램이 실행되기전 Services에 등록했던 모든 객체 정보를 알고있는 관리자
     * ServiceProvider는 객체의 생성자에 필요한 매개변수가 builder.Services에 등록돼있으면 알아서 매개변수를 찾아 주입해줌
     * dbInitializer를 ServiceProvider에게 요청함 */
    IDbInitializer dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    // 생성된 dbInitializer의 InitializeDbAsync()를 실행
    await dbInitializer.InitializeDbAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
