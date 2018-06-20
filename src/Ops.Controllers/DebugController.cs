using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Service;

namespace Ocuda.Ops.Controllers
{
    public class DebugController : ControllerBase
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        public DebugController(InsertSampleDataService insertSampleDataService)
        {
            _insertSampleDataService = insertSampleDataService 
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
        }

        public async Task<IActionResult> InsertSampleData()
        {
            await _insertSampleDataService.InsertDataAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
