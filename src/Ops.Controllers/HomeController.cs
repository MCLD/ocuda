using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Home;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Controllers
{
    [Route("")]
    public class HomeController : BaseController<HomeController>
    {
        public static string Name { get { return "Home"; } }

        public HomeController(ServiceFacades.Controller<HomeController> context) : base(context)
        {
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
