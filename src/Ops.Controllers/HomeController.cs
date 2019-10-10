using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Clc.Polaris.Api.Models;
using Clc.Polaris.Api;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Helpers;
using Ops.Service;
using System;
using System.Text;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers
{
    [Route("")]
    public class HomeController : BaseController<HomeController>
    {
        public static string Name { get { return "Home"; } }

        private readonly PapiHelper _papiHelper;
        private readonly ICoverIssueService _coverIssueService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
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

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("[action]")]
        public IActionResult Unauthorized(string returnUrl)
        {
            return View(new UnauthorizedViewModel
            {
                ReturnUrl = returnUrl,
                Username = CurrentUsername
            });
        }

        [Route("[action]/{bibID}/{imgPath}")]
        public async Task<IActionResult> SyndeticsCover(int bibID, string imgPath)
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
                    var viewModel = new SyndeticsCoverViewModel()
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
                    return RedirectToAction(nameof(Index));
                }
            }
            ShowAlertDanger("Papi is not configured.");
            return RedirectToAction(nameof(Index));
        }

        [Route("[action]")]
        public async Task<IActionResult> SyndeticsSalutation()
        {
            return View("SyndeticsSalutation");
        }

        [HttpPost]
        public async Task<IActionResult> AddCoverIssue(CoverIssueHeader header, CoverIssueDetail detail)
        {
            if (detail.CoverIssueTypeId != 0 && header.BibID != 0)
            {
                detail.CreatedBy = header.CreatedBy = CurrentUserId;
                detail.CreatedAt = header.CreatedAt = DateTime.Now;
                await _coverIssueService.CreateNewCoverIssue(detail,header);
                return RedirectToAction(nameof(SyndeticsSalutation));
            }
            else
            {
                ShowAlertDanger("There was an error in your post");
                return null;
            }
        }

        [HttpGet("[action]")]
        public IActionResult Authenticate(string returnUrl)
        {
            // by the time we get here the user is probably authenticated - if so we can take them
            // back to their initial destination
            if (HttpContext.Items[ItemKey.Nickname] != null)
            {
                return Redirect(returnUrl);
            }

            TempData[TempDataKey.AlertWarning]
                = $"Could not authenticate you for access to {returnUrl}.";
            return RedirectToAction(nameof(Index));
        }
    }
}
