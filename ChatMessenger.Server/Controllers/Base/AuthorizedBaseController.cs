using ChatMessenger.Shared.Common;
using ChatMessenger.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ChatMessenger.Server.Controllers.Base
{
    /// <summary>
    /// 로그인 정보가 필요한 Controller들이 상속받는 BaseController입니다.
    /// </summary>
    [Authorize]
    public abstract class AuthorizedBaseController : BaseController
    {
        /// <summary>
        /// 현재 로그인중인 유저의 Email (인증 필터를 통과하므로 무조건 null이 아님이 보장되기때문에 끝에 !를 붙임)
        /// </summary>
        protected string CurrentUserEmail => User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        /// <summary>
        /// 현재 로그인중인 유저의 Nickname (인증 필터를 통과하므로 무조건 null이 아님이 보장되기때문에 끝에 !를 붙임)
        /// </summary>
        protected string CurrentUserNickname => User.FindFirst(ClaimTypes.Name)?.Value!;

        protected AuthorizedBaseController() : base() { }

        /// <inheritdoc/>
        /// <remarks>
        /// 로그인 상태인지 유효성 검사를 실시합니다.<br/>
        /// 로그인 정보를 찾을수 없으면 context.Result에 UnauthorizedObjectResult를 집어넣어서 API 실행을 취소하고,<br/>
        ///  Client에게 Unauthorized 응답을 전송합니다.
        /// </remarks>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (string.IsNullOrEmpty(CurrentUserEmail))
            {
                context.Result = new UnauthorizedObjectResult("로그인 정보를 찾을 수 없거나 인증 세션이 만료됐습니다.");
                return;
            }
        }
    }
}
