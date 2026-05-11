using Microsoft.AspNetCore.Mvc;

namespace ChatMessenger.Server.Controllers.Base
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected BaseController() { }
    }
}
