using System;
using System.Diagnostics;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;
using System.Threading.Tasks;

namespace Ocuda.Promenade.Controllers
{
    [Route("[controller]")]
    public class ErrorController : BaseController
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly RedirectService _redirectService;

        public ErrorController(ILogger<ErrorController> logger,
            RedirectService redirectService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    if (redirect.IsPermanent)
                    {
                        return RedirectPermanent(redirect.Url);
                    }
                    else
                    {
                        return Redirect(redirect.Url);
                    }
                }

                _logger.LogWarning($"HTTP Error {id}: {originalPath}");
                return View("PageNotFound");
            }

            _logger.LogCritical($"HTTP Error {id}: {originalPath}");
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
