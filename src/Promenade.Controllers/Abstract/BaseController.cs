using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{

    public abstract class BaseController<T> : Controller
    {
        protected readonly ILogger _logger;
        protected readonly SiteSettingService _siteSettingService;
        protected BaseController(ServiceFacades.Controller<T> context)
        {
            _logger = context.Logger;
            _siteSettingService = context.SiteSettingService;
        }

        protected async Task<string> GetCanonicalUrl()
        {
            var isTLS = await _siteSettingService.GetSettingBoolAsync(
                Models.Keys.SiteSetting.Site.IsTLS);

            var scheme = isTLS ? "https" : "http";

            return Url.Action(null, null, null, scheme);
        }
    }
}
