using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.i18n.Filter;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Models.Keys;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{
    [ServiceFilter(typeof(LocalizationFilterAttribute), Order = 10)]
    [ServiceFilter(typeof(LayoutFilter), Order = 20)]
    [MiddlewareFilter(typeof(i18n.Middleware.LocalizationMiddleware))]
    public abstract class BaseController<T> : Controller
    {
        protected readonly ILogger<T> _logger;
        protected readonly IConfiguration _config;
        protected readonly IStringLocalizer<i18n.Resources.Shared> _sharedLocalizer;
        protected readonly SiteSettingService _siteSettingService;

        protected string PageTitle { get; set; }

        protected BaseController(ServiceFacades.Controller<T> context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            _logger = context.Logger;
            _config = context.Config;
            _sharedLocalizer = context.SharedLocalizer;
            _siteSettingService = context.SiteSettingService;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            var titleSuffix = context?.HttpContext.Items[ItemKey.PageTitleSuffix] as string;

            string pageTitle = !string.IsNullOrWhiteSpace(titleSuffix)
                ? titleSuffix
                : string.Empty;

            if (context?.Controller is BaseController<T> controller
                && !string.IsNullOrWhiteSpace(controller.PageTitle))
            {
                pageTitle = !string.IsNullOrEmpty(titleSuffix)
                    && !titleSuffix.Equals(controller.PageTitle,
                        StringComparison.OrdinalIgnoreCase)
                    ? $"{controller.PageTitle} - {titleSuffix}"
                    : controller.PageTitle;
            }

            ViewData[Utility.Keys.ViewData.Title] = pageTitle;
        }

        protected async Task<string> GetCanonicalUrl()
        {
            var isTLS = await _siteSettingService.GetSettingBoolAsync(SiteSetting.Site.IsTLS);

            var scheme = isTLS ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

            return Url.Action(null, null, null, scheme);
        }
    }
}
