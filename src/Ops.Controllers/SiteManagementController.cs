using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[controller]")]
    public class SiteManagementController : BaseController<SiteManagementController>
    {
        public static string Name { get { return "SiteManagement"; } }

        public SiteManagementController(ServiceFacades.Controller<SiteManagementController> context)
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
