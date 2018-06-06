using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Filter;

namespace Ocuda.Ops.Controllers.Abstract
{
    [ServiceFilter(typeof(OpsFilter))]
    public abstract class BaseController : Microsoft.AspNetCore.Mvc.Controller
    {
    }
}
