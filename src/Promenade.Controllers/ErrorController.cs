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
    [Route("{culture:cultureConstraint?}/[Controller]")]
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
                var redirect
                    = await _redirectService.GetUrlRedirectByPathAsync(originalPath);

                if (redirect != null)
                {
                    return redirect.IsPermanent
                        ? RedirectPermanent(redirect.Url + statusFeature?.OriginalQueryString)
                        : Redirect(redirect.Url + statusFeature?.OriginalQueryString);
                }

                _logger.LogWarning("HTTP Error {StatusCode}: {RequestPath}",
                    id,
                    originalPath);

                PageTitle = "Page not found";

                return NotFound();
            }

            _logger.LogCritical("HTTP Error {StatusCode}: {RequestPath}",
                id,
                originalPath);

            PageTitle = "Error";

            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
