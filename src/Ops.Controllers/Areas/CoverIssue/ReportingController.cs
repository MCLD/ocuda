using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Controllers.Areas.CoverIssue
{
    [Area("CoverIssue")]
    [Route("[area]/[controller]")]
    public class ReportingController : BaseController<ReportingController>
    {
        private readonly ICoverIssueService _coverIssueService;

        public static string Name { get { return "Reporting"; } }

        public ReportingController(ServiceFacades.Controller<ReportingController> context,
            ICoverIssueService coverIssueService) : base(context)
        {
            _coverIssueService = coverIssueService
                ?? throw new ArgumentNullException(nameof(coverIssueService));
        }

        [Route("{bibId}")]
        public async Task<IActionResult> Index(int bibId)
        {
            try
            {
                await _coverIssueService.AddCoverIssueAsync(bibId);
                ShowAlertSuccess("The cover issue has been reported!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording cover issue for BibId {bibId}: {ex}",
                    bibId, ex.Message);
                ShowAlertDanger("There was an error reporting this cover issue.");
            }

            return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
        }
    }
}
