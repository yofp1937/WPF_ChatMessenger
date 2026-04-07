using ChatMessenger.Server.Configs;
using ChatMessenger.Server.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 서비스 등록
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// "/Configs/DataConfig"의 AddDataServices 메서드 실행하여 DB 관련 서비스 등록
builder.Services.AddDataServices(builder.Configuration);
// "/Configs/JWTConfig"의 AddJwtAuthentication 메서드 실행하여 JWT 관련 설정 실행
builder.Services.AddJwtAuthentication(builder.Configuration);
// "/Configs/ServiceConfig"의 AddBusinessServices 메서드 실행하여 다양한 서비스들 등록
builder.Services.AddBusinessServices();

var app = builder.Build();

/* 서버 시작시 DB 초기화 로직 실행
 * using을 사용해서 scope를 잠깐 생성하여 사용한 후 내부 로직이 종료되면 메모리 해제 
 * TODO: 추후 class 만들어서 코드 줄이면 좋을듯 */
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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
