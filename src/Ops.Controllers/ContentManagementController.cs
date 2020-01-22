using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[controller]")]
    public class ContentManagementController :BaseController<ContentManagementController>
    {
        public static string Name { get { return "ContentManagment"; } }

        public ContentManagementController(ServiceFacades.Controller<ContentManagementController> context)
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
