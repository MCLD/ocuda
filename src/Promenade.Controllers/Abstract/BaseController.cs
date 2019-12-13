using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Ocuda.i18n.Filter;
using Ocuda.Promenade.Controllers.Filters;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{
    [ServiceFilter(typeof(LocalizationFilterAttribute), Order = 10)]
    [ServiceFilter(typeof(NavigationFilter), Order = 20)]
    [MiddlewareFilter(typeof(i18n.Middleware.LocalizationMiddleware))]
    public abstract class BaseController<T> : Controller
    {
        protected readonly ILogger _logger;
        protected readonly IConfiguration _config;
        protected readonly IStringLocalizer<i18n.Resources.Shared> _sharedLocalizer;
        protected readonly SiteSettingService _siteSettingService;
        protected BaseController(ServiceFacades.Controller<T> context)
        {
            _logger = context.Logger;
            _config = context.Config;
            _sharedLocalizer = context.SharedLocalizer;
            _siteSettingService = context.SiteSettingService;
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
