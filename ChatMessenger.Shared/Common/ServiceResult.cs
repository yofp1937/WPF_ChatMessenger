using ChatMessenger.Shared.Enums;

namespace ChatMessenger.Shared.Common
{
    /// <summary>
    /// Service에서 값을 반환해줄때 사용하는 클래스입니다.<br/>
    /// 요청 완료 여부, 반환해줄 데이터, 에러 메세지를 포함합니다.
    /// </summary>
    /// <typeparam name="T">반환할 데이터 타입</typeparam>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public ServiceResultType ResultType { get; }
        public string ErrorMessage { get; }

        private readonly T? _data;
        public T Data
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException($"실패한 {nameof(ServiceResult<T>)}에서는 Data에 접근할 수 없습니다. (에러:{ResultType}_{ErrorMessage}");
                // IsSuccess가 true면 !를 붙여서 컴파일러에게 null이 아님을 보장
                return _data!;
            }
        }

        /// <summary>
        /// 서비스 처리 결과를 담은 객체를 초기화합니다.
        /// </summary>
        /// <param name="isSuccess">요청의 성공 여부</param>
        /// <param name="data">반환할 데이터</param>
        /// <param name="resultType">결과 상태 코드</param>
        /// <param name="errorMessage">에러 메세지</param>
        private ServiceResult(bool isSuccess, T? data, ServiceResultType resultType, string errorMessage)
        {
            IsSuccess = isSuccess;
            _data = data;
            ResultType = resultType;
            ErrorMessage = errorMessage;
        }
        /// <summary>
        /// 요청을 성공적으로 수행했을때 결과 데이터와 성공 코드를 포함하는 객체를 생성합니다.
        /// </summary>
        /// <param name="data">요청 성공 결과 데이터</param>
        /// <returns>성공 상태의 ServiceResult 객체</returns>
        public static ServiceResult<T> Success(T data)
            => new(true, data, ServiceResultType.Success, string.Empty);
        /// <summary>
        /// 요청 수행중 오류가 발생했거나, 실패했을때 에러 메세지와 실패 코드를 포함하는 객체를 생성합니다.
        /// </summary>
        /// <param name="message">클라이언트에게 전달할 구체적인 에러 메세지</param>
        /// <param name="resultType">실패 원인을 분류하는 코드 타입</param>
        /// <returns>실패 상태의 ServiceReturn 객체</returns>
        public static ServiceResult<T> Failed(string message, ServiceResultType resultType)
            => new(false, default, resultType, message);
    }
}
