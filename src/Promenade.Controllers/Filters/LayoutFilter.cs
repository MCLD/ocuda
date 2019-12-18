using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocuda.i18n;
using Ocuda.Promenade.Models.Keys;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Filters
{
    public class LayoutFilter : IAsyncResourceFilter
    {
        private readonly ILogger<LayoutFilter> _logger;
        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;
        private readonly ExternalResourceService _externalResourceService;
        private readonly NavigationService _navigationService;
        private readonly SiteSettingService _siteSettingService;

        public LayoutFilter(ILogger<LayoutFilter> logger,
            IOptions<RequestLocalizationOptions> l10nOptions,
            ExternalResourceService externalResourceService,
            NavigationService navigationService,
            SiteSettingService siteSettingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _l10nOptions = l10nOptions ?? throw new ArgumentNullException(nameof(l10nOptions));
            _externalResourceService = externalResourceService
                ?? throw new ArgumentNullException(nameof(externalResourceService));
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            var css = await _externalResourceService
                .GetAllAsync(Models.Entities.ExternalResourceType.CSS);
            context.HttpContext.Items[ItemKey.ExternalCSS] = css.Select(_ => _.Url).ToList();

            var js = await _externalResourceService
                .GetAllAsync(Models.Entities.ExternalResourceType.JS);
            context.HttpContext.Items[ItemKey.ExternalJS] = js.Select(_ => _.Url).ToList();

            // generate list for drop-down
            var cultureList = new Dictionary<string, string>();
            var cultureHrefLang = new SortedDictionary<string, string>
                {
                    { "x-default", Culture.DefaultName }
                };
            foreach (var culture in _l10nOptions.Value.SupportedCultures)
            {
                var text = culture.Parent != null
                    ? culture.Parent.NativeName
                    : culture.NativeName;
                cultureList.Add(text, culture.Name);
                if (!cultureHrefLang.Keys.Contains(culture.Name))
                {
                    cultureHrefLang.Add(culture.Name, culture.Name);
                    if (culture.Parent != null
                        && !cultureHrefLang.Keys.Contains(culture.Parent.Name))
                    {
                        cultureHrefLang.Add(culture.Parent.Name, culture.Parent.Name);
                    }
                }
            }
            context.HttpContext.Items[ItemKey.HrefLang] = cultureHrefLang;
            context.HttpContext.Items[ItemKey.L10n] = cultureList;


            var topNavigationId
                = await _siteSettingService.GetSettingIntAsync(Models.Keys.SiteSetting.Site.NavigationIdTop);
            if (topNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.TopNavigation]
                    = await _navigationService.GetNavigation(topNavigationId);
            }

            var middleNavigationId
                = await _siteSettingService.GetSettingIntAsync(SiteSetting.Site.NavigationIdMiddle);
            if (middleNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.MiddleNavigation]
                    = await _navigationService.GetNavigation(middleNavigationId);
            }

            var leftNavigationId
                = await _siteSettingService.GetSettingIntAsync(SiteSetting.Site.NavigationIdLeft);
            if (leftNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.LeftNavigation]
                    = await _navigationService.GetNavigation(leftNavigationId);
            }

            var footerNavigationId
                = await _siteSettingService.GetSettingIntAsync(SiteSetting.Site.NavigationIdFooter);
            if (leftNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.FooterNavigation]
                    = await _navigationService.GetNavigation(footerNavigationId);
            }

            context.HttpContext.Items[ItemKey.BannerImage]
                = await _siteSettingService.GetSettingStringAsync(SiteSetting.Site.BannerImage);
            context.HttpContext.Items[ItemKey.BannerImageAlt]
                = await _siteSettingService.GetSettingStringAsync(SiteSetting.Site.BannerImageAlt);

            context.HttpContext.Items[ItemKey.GoogleAnalyticsTrackingCode]
                = await _siteSettingService.GetSettingStringAsync(SiteSetting.Site.GoogleTrackingCode);

            await next();
        }
    }
}
