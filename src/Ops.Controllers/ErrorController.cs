using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class ErrorController : BaseController<ErrorController>
    {
        public ErrorController(ServiceFacades.Controller<ErrorController> context)
            : base(context)
        {
        }

        [Route("")]
        [Route("[action]")]
        public IActionResult Index(int id)
        {
            string originalPath = "unknown";

            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusFeature != null)
            {
                originalPath = statusFeature.OriginalPath;
            }

            if (id == 404)
            {
                _logger.LogError("HTTP Error {HttpId}: {UrlPath}",
                    id,
                    originalPath);
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }

            _logger.LogCritical("HTTP Error {HttpId}: {UrlPath}",
                id,
                originalPath);
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
