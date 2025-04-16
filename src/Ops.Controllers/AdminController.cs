using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[controller]")]
    public class AdminController : BaseController<AdminController>
    {
        public static string Name
        { get { return "Admin"; } }

        public AdminController(ServiceFacades.Controller<AdminController> context)
            : base(context)
        {
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}