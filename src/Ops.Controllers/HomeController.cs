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

        private readonly ICoverIssueService _coverIssueService;

        public HomeController(ServiceFacades.Controller<HomeController> context,
            ICoverIssueService coverIssueService) : base(context)
        {
            _coverIssueService = coverIssueService
                ?? throw new ArgumentNullException(nameof(coverIssueService));
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
