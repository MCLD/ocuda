using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Models;
using Ocuda.Utility.Services.Interfaces;
using Serilog.Context;

namespace Ocuda.Promenade.Controllers.Filters
{
    public class LayoutFilter(ExternalResourceService externalResourceService,
        ILogger<LayoutFilter> logger,
        IPathResolverService pathResolverService,
        NavigationService navigationService,
        SiteSettingService siteSettingService) : IAsyncResourceFilter
    {
        private readonly ExternalResourceService _externalResourceService = externalResourceService
            ?? throw new ArgumentNullException(nameof(externalResourceService));

        private readonly ILogger<LayoutFilter> _logger = logger
            ?? throw new ArgumentNullException(nameof(logger));

        private readonly NavigationService _navigationService = navigationService
            ?? throw new ArgumentNullException(nameof(navigationService));

        private readonly IPathResolverService _pathResolverService = pathResolverService
            ?? throw new ArgumentNullException(nameof(pathResolverService));

        private readonly SiteSettingService _siteSettingService = siteSettingService
            ?? throw new ArgumentNullException(nameof(siteSettingService));

        public Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);

            return next == null
                ? throw new ArgumentNullException(nameof(next))
                : OnResourceExecutionInternalAsync(context, next);
        }

        private async Task OnResourceExecutionInternalAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            bool forceReload = context.HttpContext.Request.Query["clearcache"] == "1";

            if (forceReload)
            {
                context.HttpContext.Items[ItemKey.ForceReload] = true;
            }

            var localNetworksCsv = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Network.LocalNetworks, forceReload);

            var isLocal = false;

            if (localNetworksCsv?.Length > 0)
            {
                foreach (var networkAddress in localNetworksCsv.Split(","))
                {
                    IPNetwork2 network = null;
                    try
                    {
                        network = IPNetwork2.Parse(networkAddress);
                    }
                    catch (ArgumentException aex)
                    {
                        _logger.LogError(aex,
                            "Error parsing network address: {Message}",
                            aex.Message);
                    }
                    if (network != null)
                    {
                        isLocal = network.Contains(context.HttpContext.Connection.RemoteIpAddress);
                        if (isLocal)
                        {
                            break;
                        }
                    }
                }
                context.HttpContext.Items[ItemKey.IsLocalNetwork] = isLocal;
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
            if (footerNavigationId > 0)
            {
                context.HttpContext.Items[ItemKey.FooterNavigation]
                    = await _navigationService.GetNavigation(footerNavigationId, forceReload);
            }

            context.HttpContext.Items[ItemKey.BannerImage] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.BannerImage, forceReload);

            context.HttpContext.Items[ItemKey.BannerImageAlt] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.BannerImageAlt, forceReload);

            context.HttpContext.Items[ItemKey.CatalogSearchLink] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.CatalogSearchLink, forceReload);

            context.HttpContext.Items[ItemKey.FooterImage] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.FooterImage, forceReload);

            context.HttpContext.Items[ItemKey.FooterImageAlt] = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Site.FooterImageAlt, forceReload);

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

            using (LogContext.PushProperty(Utility.Logging.Enrichment.IsLocalNetwork, isLocal))
            {
                await next();
            }
        }
    }
}