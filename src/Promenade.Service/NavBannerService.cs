using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class NavBannerService : BaseService<NavBannerService>
    {
        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly INavBannerImageRepository _navBannerImageRepository;
        private readonly INavBannerLinkRepository _navBannerLinkRepository;
        private readonly INavBannerLinkTextRepository _navBannerLinkTextRepository;
        private readonly INavBannerRepository _navBannerRepository;
        public NavBannerService(IHttpContextAccessor httpContextAccessor,
            LanguageService languageService,
            ILogger<NavBannerService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IConfiguration config,
            INavBannerRepository navBannerRepository,
            INavBannerImageRepository navBannerImageRepository,
            INavBannerLinkRepository navBannerLinkRepository,
            INavBannerLinkTextRepository navBannerLinkTextRepository) : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(httpContextAccessor);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(navBannerRepository);
            ArgumentNullException.ThrowIfNull(navBannerImageRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkTextRepository);

            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _cache = cache;
            _config = config;
            _navBannerRepository = navBannerRepository;
            _navBannerImageRepository = navBannerImageRepository;
            _navBannerLinkRepository = navBannerLinkRepository;
            _navBannerLinkTextRepository = navBannerLinkTextRepository;
        }

        public async Task<NavBanner> GetByIdAsync(int navBannerId, bool forceReload)
        {
            NavBanner navBanner = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string navBannerCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavBanner,
                navBannerId);

            if (cachePagesInHours > 0 && !forceReload)
            {
                navBanner = await _cache.GetObjectFromCacheAsync<NavBanner>(navBannerCacheKey);
            }


            if (navBanner == null)
            {
                navBanner = await _navBannerRepository.GetByIdAsync(navBannerId);
                if (navBanner == null)
                {
                    return null;
                }

                await _cache.SaveToCacheAsync(navBannerCacheKey, navBanner, cachePagesInHours);
            }


            var languageIds
                = await GetCurrentDefaultLanguageIdAsync(
                    _httpContextAccessor,
                    _languageService);

            var languageId = languageIds.FirstOrDefault();

            string navBannerImageCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavBannerImage,
                navBannerId,
                languageId);

            if (cachePagesInHours > 0 && !forceReload)
            {
                navBanner.NavBannerImage = await _cache.GetObjectFromCacheAsync<NavBannerImage>(navBannerImageCacheKey);
            }
                

            if (navBanner.NavBannerImage == null)
            {
                navBanner.NavBannerImage = await _navBannerImageRepository.GetByNavBannerIdAsync(navBannerId, languageId);

                if (navBanner.NavBannerImage == null)
                {
                    _logger.LogError($"No image found for navBanner id: {navBannerId}");
                    return null;
                }
                
                await _cache.SaveToCacheAsync(navBannerImageCacheKey, navBanner.NavBannerImage, cachePagesInHours);
            }

            string navBannerLinksCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavBannerLinks,
                navBannerId);

            var navBannerLinks = await _cache.GetObjectFromCacheAsync<List<NavBannerLink>>(navBannerLinksCacheKey);
                
            if (navBannerLinks == null)
            {
                navBannerLinks = await _navBannerLinkRepository.GetByNavBannerIdAsync(navBannerId);

                if (!navBannerLinks.Any())
                {
                    _logger.LogError($"No links found for navBanner id: {navBannerId}");
                    return null;
                }

                await _cache.SaveToCacheAsync(navBannerLinksCacheKey, navBannerLinks, cachePagesInHours);
            }

            foreach (var link in navBannerLinks)
            {
                var navBannerLinkTextCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavBannerLinkText,
                link.Id,
                languageId);

                link.Text = await _cache.GetObjectFromCacheAsync<NavBannerLinkText>(navBannerLinkTextCacheKey);

                if (link.Text == null)
                {
                    link.Text = await _navBannerLinkTextRepository.GetByLinkIdAsync(link.Id, languageId);

                    if (link.Text == null)
                    {
                        _logger.LogError($"No link text found for navBanner link id: {link.Id}");
                        return null;
                    }

                    await _cache.SaveToCacheAsync(navBannerLinkTextCacheKey, link.Text, cachePagesInHours);
                }

            }

            navBanner.NavBannerLinks = navBannerLinks;

            return navBanner;
        }
    }
}
