using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Enums;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace ChatMessenger.Client.Common.Services.Base
{
    /// <summary>
    /// HttpClient 기반의 Client측 Service들이 상속받는 최상위 BaseService입니다.
    /// </summary>
    public abstract class BaseService
    {
        /// <summary>
        /// 모든 Http 통신과 try-catch 예외 처리를 이곳에서 관리합니다.
        /// </summary>
        /// <typeparam name="TResponse">서버에게 반환받은 Response 객체</typeparam>
        /// <typeparam name="TModel">ViewModel에게 반환해줄 Data Model 타입</typeparam>
        /// <param name="sendRequestFunc">실제 실행할 HttpClient 통신 함수</param>
        /// <param name="mapToModelFunc">HttpStatusCode가 200(Ok)으로 넘어올시 실행될 Response 객체를 Model 객체로 변환시킬 메서드</param>
        /// /// <param name="callerMemberName">컴파일러가 자동으로 주입해 주는 호출 메서드명</param>
        /// <returns></returns>
        protected async Task<ServiceResult<TModel>> ExecuteAsync<TResponse, TModel>(
            Func<Task<HttpResponseMessage>> sendRequestFunc,
            Func<TResponse, TModel> mapToModelFunc,
            [CallerMemberName] string callerMemberName = "") // 어느 Class의 어떤 Method에서 오류가 발생했는지 확인하기 위해 사용
        {
            try
            {
                // 1. 매개변수로 전달받은 HttpClient 통신 함수 실행
                HttpResponseMessage httpResponse = await sendRequestFunc();

                // 2. 서버 응답코드가 200(Ok)으로 넘어온 경우
                if (httpResponse.IsSuccessStatusCode)
                {
                    // 2-1. 서버에서 반환해준 Response 추출
                    TResponse? response = await httpResponse.Content.ReadFromJsonAsync<TResponse>();
                    if (response == null)
                        return ServiceResult<TModel>.Failed("서버 응답 response를 해석하지 못했습니다.", ServiceResultType.InternalServerError);

                    // 2-2. Model로 변환하여 반환해줌
                    TModel model = mapToModelFunc(response);
                    return ServiceResult<TModel>.Success(model);
                }

                // 3. 서버 응답코드가 실패(400, 401, 403 등)로 넘어온 경우
                return await HandleFailureResponseAsync<TModel>(httpResponse);
            }
            catch (HttpRequestException ex)
            {
                // 통신 예외 처리
                return HandleHttpRequestException<TModel>(ex, callerMemberName);
            }
            catch (Exception ex)
            {
                // 런타임 예외 처리
                return HandleGeneralException<TModel>(ex, callerMemberName);
            }
        }

        /// <summary>
        /// 서버 응답코드가 실패(400, 401, 403 등)로 넘어온 경우 ErrorMessage와 ErrorType을 추출하여 ServiceResult<TModel> 객체를 실패 상태로 생성하여 반환해줍니다.
        /// </summary>
        /// <typeparam name="TModel">ViewModel에게 반환해줄 Data Model 타입</typeparam>
        /// <param name="httpResponse">HttpClient 통신을 실행한 후 서버로부터 반환받은 Response</param>
        /// <returns>ErrorMessage와 ErrorType이 포함된 Failed 상태의 ServiceResult</returns>
        private async Task<ServiceResult<TModel>> HandleFailureResponseAsync<TModel>(HttpResponseMessage httpResponse)
        {
            // 1. ErrorMessage 추출
            string errorMessage = await httpResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = "알 수 없는 서버 오류가 발생했습니다.";

            // 2. ErrorType 추출하고 ServiceResultType으로 매핑
            ServiceResultType errorType = httpResponse.StatusCode switch
            {
                HttpStatusCode.BadRequest => ServiceResultType.BadRequest,
                HttpStatusCode.Unauthorized => ServiceResultType.Unauthorized,
                HttpStatusCode.Forbidden => ServiceResultType.Forbidden,
                HttpStatusCode.NotFound => ServiceResultType.NotFound,
                HttpStatusCode.InternalServerError => ServiceResultType.InternalServerError,
                _ => ServiceResultType.InternalServerError
            };
            // 3. ServiceResult 객체를 Failed 상태로 생성하고 ErrorMessage, ErrorType 주입하여 반환
            return ServiceResult<TModel>.Failed(errorMessage, errorType);
        }

        /// <summary>
        /// 네트워크 통신 중 오류가 발생하여 예외 처리됐을때 동작하는 예외 처리 메서드입니다. 
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="ex">해당 메서드를 호출한 Method 정보</param>
        private ServiceResult<TModel> HandleHttpRequestException<TModel>(HttpRequestException ex, string callerMethodName)
        {
            LogError(ex, callerMethodName);
            return ServiceResult<TModel>.Failed("네트워크 연결이 원할하지 않습니다. 인터넷 연결 상태를 확인해주세요.", ServiceResultType.InternalServerError);
        }

        /// <summary>
        /// 클라이언트 내부에서 오류가 발생해 예외 처리됐을떄 동작하는 예외 처리 메서드입니다.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="ex"></param>
        private ServiceResult<TModel> HandleGeneralException<TModel>(Exception ex, string callerMethodName)
        {
            LogError(ex, callerMethodName);
            return ServiceResult<TModel>.Failed("클라이언트 내부 오류가 발생했습니다.", ServiceResultType.InternalServerError);
        }

        /// <summary>
        /// 예외 처리 메서드들이 동작할때 Debug 로그를 일관된 Format으로 출력해줍니다.<br/>
        /// 추후 오류 발생시 Debug.WriteLine 말고 Log 파일을 저장하고싶을떄 이곳을 수정하면 됩니다.
        /// </summary>
        /// <param name="ex">발생한 예외 객체</param>
        /// <param name="callerMethodName">예외 처리 메서드를 발생시킨 Method의 이름</param>
        private void LogError(Exception ex, string callerMethodName)
        {
            // 1. GetType().Name으로 해당 인스턴스를 작동시키는 자식 클래스명을 추출
            string className = GetType().Name;
            // 2. 오류가 발생하면 "[클래스명_메서드명]: 에러 메세지" 형식으로 로그 남김
            Debug.WriteLine($"[{className}_{callerMethodName}]: {ex.Message}");
        }
    }
}
