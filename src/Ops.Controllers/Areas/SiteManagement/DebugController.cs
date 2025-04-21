using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class DebugController : BaseController<DebugController>
    {
        private readonly IInsertSampleDataService _insertSampleDataService;

        public DebugController(ServiceFacades.Controller<DebugController> context,
            IInsertSampleDataService insertSampleDataService) : base(context)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
        }

        [Route("[action]")]
        public async Task<IActionResult> InsertSampleData()
        {
            await _insertSampleDataService.InsertDataAsync();

            // force user to re-load permissions now that there are new sections
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
