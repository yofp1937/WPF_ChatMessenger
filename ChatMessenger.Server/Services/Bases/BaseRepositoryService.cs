using ChatMessenger.Server.Data;
using ChatMessenger.Server.Interfaces.Services.Repositories;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace ChatMessenger.Server.Services.Bases
{
    /// <summary>
    /// Db에 접근하여 직접적인 데이터 처리를 담당하는 RepositoryService들이 반드시 상속받아야하는 최상위 부모 Class입니다.
    /// </summary>
    /// <remarks>
    /// 자식 Class에서 Db Context에 접근할 수 있도록 공통 기능을 제공합니다.
    /// </remarks>
    public abstract class BaseRepositoryService : IBaseRepositoryService
    {
        protected readonly AppDbContext _context;
        /// <summary>
        /// 표준 로깅을 위한 로거입니다. 자식 Class가 자신의 타입(ILogger&lt;자식타입&gt;)을 주입하므로,
        /// 로그 카테고리에 실제 예외가 발생한 구체 Class명이 자동으로 기록됩니다.
        /// </summary>
        protected readonly ILogger _logger;

        protected BaseRepositoryService(AppDbContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }
        #region public Method
        /// <inheritdoc/>
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        #endregion public Method
        #region protected Method
        /// <summary>
        /// Db 데이터 처리 작업을 try-catch를 사용해 안전하게 실행하고, 예외 발생시 로그를 남긴 후 throw합니다.<br/>
        /// </summary>
        /// <remarks>
        /// 1. Repository의 Data 반환, 예외 처리 등 여러 작업들을 일관되게 적용하기위해 사용합니다.<br/>
        /// 2. 내부에서 예외가 발생하면 로그로 기록하고, 상위 비즈니스 Service에서 처리할 수 있도록 throw됩니다.
        /// </remarks>
        /// <typeparam name="T">작업 처리 후 반환할 타입</typeparam>
        /// <param name="action">try 내부에서 실행할 Db 작업</param>
        /// <param name="callerMethodName">컴파일러에 의해 주입되는 호출 메서드 이름</param>
        /// <returns>Db 작업의 결과 값</returns>
        protected async Task<T> ExecuteDbActionAsync<T>(Func<Task<T>> action, [CallerMemberName] string callerMethodName = "")
        {
            try
            {
                // 넘겨받은 Db 로직 실행
                return await action();
            }
            catch (Exception ex)
            {
                // 에러 발생시 로그 기록
                LogRepositoryError(ex, callerMethodName);
                // 상위 서비스로 throw
                throw;
            }
        }
        #endregion protected Method
        #region private Method
        /// <summary>
        /// Db 작업 중 발생한 예외를 로그로 남깁니다.
        /// </summary>
        /// <param name="ex">발생한 예외 객체</param>
        /// <param name="callerMethodName">컴파일러에 의해 주입되는 호출 메서드 이름</param>
        private void LogRepositoryError(Exception ex, string callerMethodName = "")
        {
            // 1. GetType().Name으로 해당 인스턴스를 작동시키는 자식 클래스명을 추출
            string className = GetType().Name;
            // 2. ILogger로 구조적 로그 기록.
            //    - 타임스탬프/로그레벨/카테고리는 로깅 프레임워크가 자동 부여하므로 수기 포맷팅을 제거함.
            //    - 예외 객체(ex)를 첫 인자로 넘겨 스택 트레이스까지 함께 기록되도록 함.
            _logger.LogError(ex, "[Database Error] [{ClassName}.{Method}] Db 작업 중 예외가 발생했습니다.",
                className, callerMethodName);
        }
        #endregion private Method
    }
}
