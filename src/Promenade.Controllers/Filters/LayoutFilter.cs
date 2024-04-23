using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Controllers.Filters
{
    public class LayoutFilter : IAsyncResourceFilter
    {
        private readonly ExternalResourceService _externalResourceService;
        private readonly NavigationService _navigationService;
        private readonly IPathResolverService _pathResolverService;
        private readonly SiteSettingService _siteSettingService;

        public LayoutFilter(ExternalResourceService externalResourceService,
            NavigationService navigationService,
            IPathResolverService pathResolverService,
            SiteSettingService siteSettingService)
        {
            _externalResourceService = externalResourceService
                ?? throw new ArgumentNullException(nameof(externalResourceService));
            _navigationService = navigationService
                ?? throw new ArgumentNullException(nameof(navigationService));
            _pathResolverService = pathResolverService
                ?? throw new ArgumentNullException(nameof(pathResolverService));
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

            if (forceReload)
            {
                context.HttpContext.Items[ItemKey.ForceReload] = true;
            }

            context.HttpContext.Items[ItemKey.PublicContentPath]
                = _pathResolverService.GetPublicContentLink();

            var externalResources = await _externalResourceService.GetAllAsync(forceReload);

            context.HttpContext.Items[ItemKey.ExternalCSS] = externalResources
                .Where(_ => _.Type == ExternalResourceType.CSS)
                .Select(_ => _.Url).ToList();

            context.HttpContext.Items[ItemKey.ExternalJS] = externalResources
                .Where(_ => _.Type == ExternalResourceType.JS)
                .Select(_ => _.Url).ToList();

            context.HttpContext.Items[ItemKey.PageTitleSuffix] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.PageTitleSuffix, forceReload);

            var topNavigationId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Site.NavigationIdTop, forceReload);

            if (topNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.TopNavigation]
                    = await _navigationService.GetNavigation(topNavigationId, forceReload);
            }

            var middleNavigationId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Site.NavigationIdMiddle, forceReload);

            if (middleNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.MiddleNavigation]
                    = await _navigationService.GetNavigation(middleNavigationId, forceReload);
            }

            var leftNavigationId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Site.NavigationIdLeft, forceReload);

            if (leftNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.LeftNavigation]
                    = await _navigationService.GetNavigation(leftNavigationId, forceReload);
            }

            var footerNavigationId = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Site.NavigationIdFooter, forceReload);
            if (leftNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.FooterNavigation]
                    = await _navigationService.GetNavigation(footerNavigationId, forceReload);
            }

            context.HttpContext.Items[ItemKey.BannerImage] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.BannerImage, forceReload);

            context.HttpContext.Items[ItemKey.BannerImageAlt] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.BannerImageAlt, forceReload);

            context.HttpContext.Items[ItemKey.GoogleAnalyticsTrackingCode]
                = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Site.GoogleTrackingCode,
                        forceReload);

            context.HttpContext.Items[ItemKey.SocialFacebookUrl] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Social.FacebookUrl, forceReload);

            context.HttpContext.Items[ItemKey.SocialInstagramUrl] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Social.InstagramUrl, forceReload);

            context.HttpContext.Items[ItemKey.SocialTikTokUrl] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Social.TikTokUrl, forceReload);

            context.HttpContext.Items[ItemKey.SocialTwitterUrl] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Social.TwitterUrl, forceReload);

            context.HttpContext.Items[ItemKey.SocialYoutubeUrl] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Social.YoutubeUrl, forceReload);

            context.HttpContext.Items[ItemKey.Telephone] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Contact.Telephone, forceReload);

            context.HttpContext.Items[ItemKey.ContactLink] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Contact.Link, forceReload);

            await next();
        }
    }
}