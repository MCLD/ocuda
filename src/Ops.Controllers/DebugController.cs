using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers
{
    public class DebugController : ControllerBase
    {
        private readonly IInsertSampleDataService _insertSampleDataService;
        public DebugController(IInsertSampleDataService insertSampleDataService)
        {
            _insertSampleDataService = insertSampleDataService 
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
        }

        public async Task<IActionResult> InsertSampleData()
        {
            await _insertSampleDataService.InsertDataAsync();

            // force user to re-load permissions now that there are new sections
            await HttpContext.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
