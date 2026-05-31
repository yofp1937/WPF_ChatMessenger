using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChatMessenger.Server.Controllers.Base
{
    [ApiController]
    public abstract class BaseController : ControllerBase, IActionFilter
    {
        protected BaseController() { }

        /// <summary>
        /// Action Method 실행 직전 호출되는 가상 메서드입니다.<br/>
        /// * Action Method: Controller 내부에서 외부 Http 요청을 받아 응답(IActionResult)을 주는 API 함수들을 의미
        /// </summary>
        /// <param name="context"></param>
        [NonAction] // API 주소로 노출되는것 방지
        public virtual void OnActionExecuting(ActionExecutingContext context) { }

        /// <summary>
        /// Action Method 실행 직후 호출되는 가상 메서드입니다.
        /// * Action Method: Controller 내부에서 외부 Http 요청을 받아 응답(IActionResult)을 주는 API 함수들을 의미
        /// </summary>
        /// <param name="context"></param>
        [NonAction]
        public virtual void OnActionExecuted(ActionExecutedContext context) { }

        /// <summary>
        /// Client의 요청을 처리하여 생성된 ServiceResult의 값에따라 최적의 Http Status Code를 생성하여 반환합니다. 
        /// </summary>
        /// <typeparam name="T">ServiceResult 객체가 가지고있는 내부 Data의 타입</typeparam>
        /// <param name="result">Service로부터 전달받은 요청 처리 결과가 담긴 객체</param>
        /// <returns>결과에 따른 Http Status Code와 Data 혹은 ErrorMessage</returns>
        protected IActionResult ContextResponse<T>(ServiceResult<T> result)
        {
            return result.ResultType switch
            {
                ServiceResultType.Success => Ok(result.Data),
                ServiceResultType.BadRequest => BadRequest(result.ErrorMessage),
                ServiceResultType.Forbidden => Forbid(),
                ServiceResultType.NotFound => NotFound(result.ErrorMessage),
                ServiceResultType.InternalServerError => StatusCode(StatusCodes.Status500InternalServerError, result.ErrorMessage),
                // 혹시 모를 예외 처리
                _ => BadRequest(result.ErrorMessage)
            };
        }
    }
}
