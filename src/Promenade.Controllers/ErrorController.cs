using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                var queryParams = Request.Query.Select(_ => new KeyValuePair<string, string>
                (
                    _.Key,
                    _.Value
                )).ToList();

                var redirect = await _redirectService.GetUrlRedirectByPathAsync(originalPath,
                    queryParams);
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
