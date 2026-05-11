using Microsoft.AspNetCore.Authorization;

namespace ChatMessenger.Server.Controllers.Base
{
    /// <summary>
    /// 누구나 접근 가능한 Controller들이 상속받는 BaseController입니다.
    /// </summary>
    [AllowAnonymous]
    public abstract class AnonymousBaseController : BaseController
    {
        protected AnonymousBaseController() : base() { }
    }
}
