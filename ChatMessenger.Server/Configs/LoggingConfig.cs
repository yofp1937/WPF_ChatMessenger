/*
 * Serilog 기반 로깅 파이프라인을 구성하는 클래스
 */
using Serilog;
using Serilog.Events;

namespace ChatMessenger.Server.Configs
{
    public static class LoggingConfig
    {
        /// <summary>
        /// 실행 환경(Development/Production)에 따라 로그 Provider를 분리 등록합니다.
        /// </summary>
        /// <remarks>
        /// Development: Console/Debug 출력창에만 로그를 남기고 파일에는 기록하지 않습니다.<br/>
        /// Production: Console(docker logs 확인용)에 더해, Error 이상 로그는 파일로 영속 저장합니다.<br/>
        /// 파일 쓰기는 요청 처리 스레드를 블로킹하지 않도록 Async Sink로 감쌉니다.<br/>
        /// AIPrompt/BackEnd.md §7(로그 영속화 및 실행 환경별 Provider 분리 정책)에 따른 구성입니다.
        /// </remarks>
        public static void AddSerilogLogging(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, loggerConfig) =>
            {
                loggerConfig
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext();

                if (context.HostingEnvironment.IsDevelopment())
                {
                    // 로컬 개발: Console/Debug 출력창에만 기록, 파일에는 남기지 않음
                    loggerConfig
                        .WriteTo.Console()
                        .WriteTo.Debug();
                }
                else
                {
                    // 운영: 콘솔(docker logs 확인용) + Error 이상 로그는 파일로 영속 저장
                    loggerConfig
                        .WriteTo.Console()
                        .WriteTo.Async(sinkConfig => sinkConfig.File(
                            path: "logs/chatmessenger-.log",
                            restrictedToMinimumLevel: LogEventLevel.Error,
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 30));
                }
            });
        }
    }
}
