using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.CoverIssueManagement;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class CoverIssueManagementController : BaseController<CoverIssueManagementController>
    {
        public static string Name { get { return "CoverIssueManagement"; } }
        private readonly ICoverIssueService _coverIssueService;
        private readonly IConfiguration _config;

        public CoverIssueManagementController(
            ServiceFacades.Controller<CoverIssueManagementController> context,
            ICoverIssueService coverIssueService,
            IConfiguration config) : base(context)
        {
            _coverIssueService = coverIssueService
                ?? throw new ArgumentNullException(nameof(coverIssueService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(string search, int orderBy, bool orderDesc,
            int page = 1)
        {
            search = search?.Trim();
            var filter = new CoverIssueHeaderFilter(page)
            {
                Search = search,
                OrderBy = (CoverIssueHeaderFilter.OrderType)orderBy,
                OrderDesc = orderDesc
            };
            var headers = await _coverIssueService.PageHeaderItemsAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = headers.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new
                {
                    page = paginateModel.LastPage ?? 1
                });
            }

            return View(new CoverIssueManagementViewModel
            {
                AllCoverIssues = headers.Data,
                PaginateModel = paginateModel,
                OrderBy = orderBy,
                OrderDesc = orderDesc,
                Search = search,
                SearchCount = headers.Count
            });
        }

        [Route("[action]/{bibId}")]
        [Route("{bibId}")]
        public async Task<IActionResult> CoverIssue(int bibId)
        {
            var header = _coverIssueService.GetCoverIssueHeaderByBibId(bibId);
            if (header == null)
            {
                ShowAlertDanger($"The BibID '{bibId}' does not have any pending or past issues.");
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var viewModel = new CoverIssueManagementViewModel
                {
                    Header = header,
                    Details = await _coverIssueService.GetCoverIssueDetailsByHeaderAsync(header.Id),
                    Types = await _coverIssueService.GetAllCoverIssueTypesAsync(),
                    CoverIssueImgPath = _config["CoverIssueImageUrl"],
                    CoverIssueLeapPath = _config["LeapRecordPath"]
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ResolveIssues(int detailId)
        {
            var detail = await _coverIssueService.GetCoverIssueDetailByIdAsync(detailId);
            var header = await _coverIssueService.GetCoverIssueHeaderByDetailIdAsync(detailId);
            var type = await _coverIssueService.GetCoverIssueTypeByIdAsync(detail.CoverIssueTypeId);
            try
            {
                await _coverIssueService.ResolveCoverIssue(detailId);
                if (!type.HasMessage)
                {
                    ShowAlertSuccess($"Resolved '{type.Name}' issues for '{header.BibID}'");
                }
                else
                {
                    ShowAlertSuccess($"Resolved issue for '{header.BibID}'");
                }
            }
            catch
            {
                if (!type.HasMessage)
                {
                    ShowAlertDanger($"Could not resolve '{type.Name}' issues for '{header.BibID}'");
                }
                else
                {
                    ShowAlertDanger($"Could not resolve issue for '{header.BibID}'");
                }
            }
            return RedirectToAction(nameof(CoverIssue), new { bibId = header.BibID });
        }
    }
}