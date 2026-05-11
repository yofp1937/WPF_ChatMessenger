using Microsoft.AspNetCore.Authorization;
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
        /// 현재 로그인중인 유저의 Email
        /// </summary>
        protected string? CurrentUserEmail => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        /// <summary>
        /// 현재 로그인중인 유저의 Nickname
        /// </summary>
        protected string? CurrentUserNickname => User.FindFirst(ClaimTypes.Name)?.Value;

        protected AuthorizedBaseController() : base() { }
    }
}
