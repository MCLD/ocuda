using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        public static string Name { get { return "Home"; } }

        public HomeController(ServiceFacades.Controller<HomeController> context)
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
