using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Ocuda.i18n;
using Ocuda.Promenade.Models.Keys;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Controllers.Filters
{
    public class LayoutFilter : IAsyncResourceFilter
    {
        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;
        private readonly ExternalResourceService _externalResourceService;
        private readonly NavigationService _navigationService;
        private readonly SiteSettingService _siteSettingService;

        public LayoutFilter(IOptions<RequestLocalizationOptions> l10nOptions,
            ExternalResourceService externalResourceService,
            NavigationService navigationService,
            SiteSettingService siteSettingService)
        {
            _l10nOptions = l10nOptions ?? throw new ArgumentNullException(nameof(l10nOptions));
            _externalResourceService = externalResourceService
                ?? throw new ArgumentNullException(nameof(externalResourceService));
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public Task OnResourceExecutionAsync(ResourceExecutingContext context,
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

            return OnResourceExecutionInternalAsync(context, next);
        }

        private async Task OnResourceExecutionInternalAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            bool forceReload = context.HttpContext.Request.Query["clearcache"] == "1";

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

            var externalResources = await _externalResourceService.GetAllAsync(forceReload);

            context.HttpContext.Items[ItemKey.ExternalCSS] = externalResources
                .Where(_ => _.Type == ExternalResourceType.CSS)
                .Select(_ => _.Url).ToList();

            context.HttpContext.Items[ItemKey.ExternalJS] = externalResources
                .Where(_ => _.Type == ExternalResourceType.JS)
                .Select(_ => _.Url).ToList();

            context.HttpContext.Items[ItemKey.PageTitleSuffix] = await _siteSettingService
                .GetSettingStringAsync(SiteSetting.Site.PageTitleSuffix, forceReload);

            var topNavigationId = await _siteSettingService
                .GetSettingIntAsync(SiteSetting.Site.NavigationIdTop, forceReload);

            if (topNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.TopNavigation]
                    = await _navigationService.GetNavigation(topNavigationId, forceReload);
            }

            var middleNavigationId = await _siteSettingService
                .GetSettingIntAsync(SiteSetting.Site.NavigationIdMiddle, forceReload);

            if (middleNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.MiddleNavigation]
                    = await _navigationService.GetNavigation(middleNavigationId, forceReload);
            }

            var leftNavigationId = await _siteSettingService
                .GetSettingIntAsync(SiteSetting.Site.NavigationIdLeft, forceReload);

            if (leftNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.LeftNavigation]
                    = await _navigationService.GetNavigation(leftNavigationId, forceReload);
            }

            var footerNavigationId = await _siteSettingService
                .GetSettingIntAsync(SiteSetting.Site.NavigationIdFooter, forceReload);
            if (leftNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.FooterNavigation]
                    = await _navigationService.GetNavigation(footerNavigationId, forceReload);
            }

            context.HttpContext.Items[ItemKey.BannerImage] = await _siteSettingService
                .GetSettingStringAsync(SiteSetting.Site.BannerImage, forceReload);

            context.HttpContext.Items[ItemKey.BannerImageAlt] = await _siteSettingService
                .GetSettingStringAsync(SiteSetting.Site.BannerImageAlt, forceReload);

            context.HttpContext.Items[ItemKey.GoogleAnalyticsTrackingCode]
                = await _siteSettingService
                    .GetSettingStringAsync(SiteSetting.Site.GoogleTrackingCode, forceReload);

            await next();
        }
    }
}
