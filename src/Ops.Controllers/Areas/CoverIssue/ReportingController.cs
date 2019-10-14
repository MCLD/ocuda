using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Clc.Polaris.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.CoverIssues.ViewModels.Reporting;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Helpers;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.CoverIssues
{
    [Area("CoverIssue")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class ReportingController : BaseController<ReportingController>
    {
        public static string Name { get { return "Reporting"; } }
        private readonly ICoverIssueService _coverIssueService;
        private readonly PapiHelper _papiHelper;

        public ReportingController(
            ServiceFacades.Controller<ReportingController> context,
            ICoverIssueService coverIssueService,
            PapiHelper papiHelper) : base(context)
        {
            _coverIssueService = coverIssueService
                ?? throw new ArgumentNullException(nameof(coverIssueService));
            _papiHelper = papiHelper;
            if (!_papiHelper.IsConfigured())
            {
                _logger.LogCritical("PAPI not configured");
            }
        }

        [Route("")]
        [Route("{bibID}")]
        [Route("{bibID}/{imgPath}")]
        public async Task<IActionResult> Index(int bibID, string imgPath)
        {
            if (_papiHelper.IsConfigured())
            {
                var papi = new PapiClient
                {
                    AccessID = _papiHelper.AccessID,
                    AccessKey = _papiHelper.AccessKey,
                    BaseUrl = _papiHelper.BaseUrl,
                    StaffOverrideAccount = _papiHelper.StaffOverrideAccount
                };
                var bibResult = papi.BibGet(bibID);
                if (bibResult.Data.PAPIErrorCode == 0 && !string.IsNullOrEmpty(bibResult.Data.Title))
                {
                    var byteData = Convert.FromBase64String(imgPath);
                    var imgStr = Encoding.UTF8.GetString(byteData);
                    var strArr = imgStr.Split("&upc=");
                    var dataVar = strArr[1].Split("&oclc=");
                    var upc = dataVar[0];
                    var oclc = "";
                    if (dataVar.Length > 1)
                    {
                        oclc = dataVar[1];
                    }
                    var viewModel = new ReportingViewModel()
                    {
                        Title = bibResult.Data.Title,
                        Author = string.Join(", ", bibResult.Data.Author),
                        ISBN = bibResult.Data.ISBN,
                        UPC = upc,
                        OCLC = oclc,
                        ItemImagePath = imgStr,
                        Format = bibResult.Data.Format,
                        Summary = string.Join(", ", bibResult.Data.Summary),
                        LCCN = bibResult.Data.LCCN,
                        BibID = bibID,
                        CoverIssueTypes = await _coverIssueService.GetAllCoverIssueTypesAsync()
                    };
                    return View(viewModel);
                }
                else
                {
                    ShowAlertDanger($"Could not find the Bib ID: {bibID}");
                    return RedirectToAction(nameof(HomeController.Index));
                }
            }
            ShowAlertDanger("Papi is not configured.");
            return RedirectToAction(nameof(HomeController.Index));
        }

        [Route("[action]")]
        public IActionResult Salutation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCoverIssue(CoverIssueHeader header, CoverIssueDetail detail)
        {
            if (detail.CoverIssueTypeId != 0 && header.BibID != 0)
            {
                detail.CreatedBy = header.CreatedBy = CurrentUserId;
                detail.CreatedAt = header.CreatedAt = DateTime.Now;
                await _coverIssueService.CreateNewCoverIssue(detail, header);
                return RedirectToAction(nameof(Salutation));
            }
            else
            {
                ShowAlertDanger("There was an error in your post");
                return null;
            }
        }
    }
}
