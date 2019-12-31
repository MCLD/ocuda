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
        protected readonly ILogger _logger;
        protected readonly IConfiguration _config;
        protected readonly IStringLocalizer<i18n.Resources.Shared> _sharedLocalizer;
        protected readonly SiteSettingService _siteSettingService;

        protected string PageTitle { get; set; }

        protected BaseController(ServiceFacades.Controller<T> context)
        {
            _logger = context.Logger;
            _config = context.Config;
            _sharedLocalizer = context.SharedLocalizer;
            _siteSettingService = context.SiteSettingService;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await base.OnActionExecutionAsync(context, next);

            string pageTitle;
            var titleSuffix = await _siteSettingService.GetSettingStringAsync(
                SiteSetting.Site.PageTitleSuffix);

            if (context.Controller is BaseController<T> controller
                && !string.IsNullOrWhiteSpace(controller.PageTitle))
            {
                if (!string.IsNullOrEmpty(titleSuffix)
                    && !titleSuffix.Equals(controller.PageTitle, StringComparison.OrdinalIgnoreCase))
                {
                    pageTitle = $"{controller.PageTitle} - {titleSuffix}";
                }
                else
                {
                    pageTitle = controller.PageTitle;
                }
            }
            else
            {
                pageTitle = !string.IsNullOrWhiteSpace(titleSuffix) ? titleSuffix : string.Empty;
            }

            ViewData[Utility.Keys.ViewData.Title] = pageTitle;

            await next();
        }

        protected async Task<string> GetCanonicalUrl()
        {
            var isTLS = await _siteSettingService.GetSettingBoolAsync(
                Models.Keys.SiteSetting.Site.IsTLS);

            var scheme = isTLS ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;

            return Url.Action(null, null, null, scheme);
        }
    }
}
