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
    public class CoverIssueController :BaseController<CoverIssueController>
    {
        public static string Name { get { return "CoverIssue"; } }

        public CoverIssueController(ServiceFacades.Controller<CoverIssueController> context)
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
