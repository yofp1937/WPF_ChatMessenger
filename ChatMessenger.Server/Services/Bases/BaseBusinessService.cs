using ChatMessenger.Server.Interfaces.Services.Repositories;
using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using System.Runtime.CompilerServices;

namespace ChatMessenger.Server.Services.Bases
{
    /// <summary>
    /// Server측 Business Logic을 처리하는 Service들이 상속받는 최상위 부모 Class입니다.
    /// </summary>
    /// <remarks>
    /// 1. Client의 요청에 따라 Data를 처리해 응답 객체(ServiceResult)로 변환하는 역할을 수행합니다.
    /// 2. 하위 Repository Service에서 던져진 예외를 처리합니다. 
    /// </remarks>
    public abstract class BaseBusinessService
    {
        #region protected Method
        /// <summary>
        /// try-catch문을 사용해 비즈니스 로직을 안전하게 실행하고, 예외 발생시 ServiceResult를 반환하면서 로그도 남깁니다.
        /// </summary>
        /// <typeparam name="T">반환할 ServiceResult의 데이터 타입</typeparam>
        /// <param name="businessLogic">try 내부에서 실행할 비지니스 로직</param>
        /// <returns>로직 실행 결과 또는 예외 처리 결과</returns>
        protected async Task<ServiceResult<T>> ExecutedBusinessLogicAsync<T>(Func<Task<ServiceResult<T>>> businessLogic)
        {
            try
            {
                return await businessLogic();
            }
            catch (Exception ex)
            {
                return HandleExcetpion<T>(ex);
            }
        }
        /// <summary>
        /// repositoryService들의 transaction 생성 메서드를 이용해 여러 Db 작업의 원자성을 보장합니다.<br/>
        /// try-cach문을 사용해 하나의 작업이 실패하면 모든 작업을 Rollback합니다.
        /// </summary>
        /// <remarks>
        /// 해당 메서드는 ExecutedBusinessLogicAsync 내부에서 호출되기때문에 예외 발생시 throw new Exception을 사용하면 정상적으로 로그가 기록됩니다.
        /// </remarks>
        /// <typeparam name="T">반환할 데이터 타입</typeparam>
        /// <param name="repositoryService">transaction 생성 메서드를 호출해줄 RepositoryService</param>
        /// <param name="transactionLogic">try 내부에서 실행될 로직</param>
        /// <returns>실행 결과 Data</returns>
        protected async Task<T> ExecuteTransactionAsync<T>(IBaseRepositoryService repositoryService,
            Func<Task<T>> transactionLogic)
        {
            using IDbContextTransaction transaction = await repositoryService.BeginTransactionAsync();
            try
            {
                // 1. 로직 실행
                T result = await transactionLogic();
                // 2. 로직에서 throw 발생 안했으면 Commit
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                // ExecuteTransactionAsync는 결국 ExecuteBusinessLogicAsync에서 실행되므로
                // 예외가 발생했을때 throw하면 ExecuteBusinessLogicAsync에서 catch하여 로그를 기록함
                throw;
            }
        }
        #endregion protected Method
        #region private Method
        /// <summary>
        /// 서버 내부에서 발생한 Exception 로그를 일관된 형식으로 남기고,<br/>
        /// ServiceResult 객체의 ResultType을 InternalServerError로 설정하여 생성합니다.
        /// </summary>
        /// <typeparam name="T">반환할 ServiceResult의 데이터 타입</typeparam>
        /// <param name="ex">발생한 예외 객체</param>
        /// <param name="errorMessage">외부로 노출할 에러 메세지</param>
        /// <param name="callerMethodName">컴파일러에 의해 주입되는 호출 메서드 이름</param>
        /// <returns>Data 요청 결과가 담긴 ServiceResult 객체</returns>
        private ServiceResult<T> HandleExcetpion<T>(Exception ex, string errorMessage = "서버 내부 데이터 처리 중 오류가 발생했습니다.",
            [CallerMemberName] string callerMethodName = "") // 어느 Class의 어떤 Method에서 오류가 발생했는지 확인하기 위해 사용
        {
            // 1. 공통 로그 처리 메서드 호출
            LogBusinessError(ex, callerMethodName);
            // 2. 서버 내부 오류 코드인 InternalServerError 반환
            return ServiceResult<T>.Failed(errorMessage, ServiceResultType.InternalServerError);
        }
        /// <summary>
        /// 발생한 Exception을 로그로 납깁니다.
        /// </summary>
        /// <param name="ex">발생한 예외 객체</param>
        /// <param name="callerMethodName">호출 메서드 이름</param>
        private void LogBusinessError(Exception ex, string callerMethodName)
        {
            // 1. GetType().Name으로 해당 인스턴스를 작동시키는 자식 클래스명을 추출
            string className = GetType().Name;
            // 2. 현재 시간 정보 확보
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            // 3. 로그 메세지 형식 작성
            string logMessage = $"[{timestamp}] [Error] [{className}_{callerMethodName}]: {ex.Message}";
            // 4. 콘솔 출력 (TODO: 추후 로그 저장 추가시 해당 부분 변경)
            Console.WriteLine(logMessage);
        }
        #endregion private Method
    }
}
