using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Serilog.Context;

namespace Ocuda.Promenade.Controllers
{
    [Route("[controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class ErrorController : BasePageController<ErrorController>
    {
        private const string ErrorMessage = "HTTP {Method} Error {StatusCode} {StatusName}: {RequestPath}";
        private readonly PageService _pageService;
        private readonly RedirectService _redirectService;

        public ErrorController(ServiceFacades.Controller<ErrorController> context,
            ServiceFacades.PageController pageContext,
            PageService pageService,
            RedirectService redirectService) : base(context, pageContext)
        {
            ArgumentNullException.ThrowIfNull(pageService);
            ArgumentNullException.ThrowIfNull(redirectService);

            _pageService = pageService;
            _redirectService = redirectService;
        }

        public static string Name
        { get { return "Error"; } }

        protected override PageType PageType
        { get { return PageType.Sorry; } }

        [HttpGet("")]
        [HttpGet("{id}")]
        [HttpPost("")]
        [HttpPost("{id}")]
        public async Task<IActionResult> Index(int id)
        {
            string originalPath = "unknown";

            var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            if (statusFeature != null)
            {
                originalPath = statusFeature.OriginalPath;
            }

            if (id == (int)HttpStatusCode.NotFound || id == default)
            {
                var redirect
                    = await _redirectService.GetUrlRedirectByPathAsync(originalPath);

                if (redirect != null)
                {
                    return redirect.IsPermanent
                        ? RedirectPermanent(redirect.Url + statusFeature?.OriginalQueryString)
                        : Redirect(redirect.Url + statusFeature?.OriginalQueryString);
                }

                _logger.LogWarning(ErrorMessage,
                    HttpContext.Request.Method,
                    id,
                    Enum.GetName(typeof(HttpStatusCode), id),
                    originalPath);

                var notFoundPageHeader
                    = await _pageService.GetHeaderByStubAndTypeAsync(Utility.ErrorPageSlug.NotFound,
                        PageType,
                        false);

                if (notFoundPageHeader != null)
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return await ReturnPageAsync(Utility.ErrorPageSlug.NotFound);
                }
                else
                {
                    PageTitle = _localizer[i18n.Keys.Promenade.ErrorPageNotFound];
                    return NotFound();
                }
            }

            using (LogContext.PushProperty("FormContent", HttpContext.Request.HasFormContentType 
                ? HttpContext.Request.Form : null))
            {
                _logger.LogCritical(ErrorMessage,
                    HttpContext.Request.Method,
                    id,
                    Enum.GetName(typeof(HttpStatusCode), id),
                    originalPath);
            }

            var errorPageHeader
                = await _pageService.GetHeaderByStubAndTypeAsync(Utility.ErrorPageSlug.Error,
                    PageType,
                    false);

            if (errorPageHeader != null)
            {
                HttpContext.Response.StatusCode = id;
                return await ReturnPageAsync(Utility.ErrorPageSlug.Error);
            }

            PageTitle = _localizer[i18n.Keys.Promenade.Error];

            return View("Error", new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}