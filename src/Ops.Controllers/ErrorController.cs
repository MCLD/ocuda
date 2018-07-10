using System;
using System.Diagnostics;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Controllers
{
    public class ErrorController : BaseController<ErrorController>
    {
        public ErrorController(ServiceFacade.Controller<ErrorController> context)
            : base(context)
        {
        }

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
                _logger.LogError($"HTTP Error {id}: {originalPath}");
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }

            _logger.LogCritical($"HTTP Error {id}: {originalPath}");
            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
