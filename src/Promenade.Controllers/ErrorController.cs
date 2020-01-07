using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[controller]")]
    public class ErrorController : BaseController<ErrorController>
    {
        private readonly RedirectService _redirectService;

        public ErrorController(ServiceFacades.Controller<ErrorController> context,
            RedirectService redirectService) : base(context)
        {
            _redirectService = redirectService
                ?? throw new ArgumentNullException(nameof(redirectService));
        }

        [HttpGet("")]
        [HttpGet("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            string originalPath = "unknown";

            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusFeature != null)
            {
                originalPath = statusFeature.OriginalPath;
            }

            if (id == 404)
            {
                var redirect = await _redirectService.GetUrlRedirectByPathAsync(originalPath);

                if (redirect != null)
                {
                    var redirectUrl = redirect.Url;

                    if (Request.QueryString.HasValue)
                    {
                        redirectUrl += Request.QueryString;
                    }

                    if (redirect.IsPermanent)
                    {
                        return RedirectPermanent(redirectUrl);
                    }
                    else
                    {
                        return Redirect(redirectUrl);
                    }
                }

                _logger.LogWarning($"HTTP Error {id}: {originalPath}");

                PageTitle = "Page not found";

                return NotFound();
            }

            _logger.LogCritical($"HTTP Error {id}: {originalPath}");

            PageTitle = "Error";

            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
