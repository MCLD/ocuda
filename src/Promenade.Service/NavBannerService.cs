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
        private const string NavBannerFilePath = "navbanner";

        private readonly IOcudaCache _cache;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly INavBannerImageRepository _navBannerImageRepository;
        private readonly INavBannerLinkRepository _navBannerLinkRepository;
        private readonly INavBannerLinkTextRepository _navBannerLinkTextRepository;
        private readonly IPathResolverService _pathResolverService;

        public NavBannerService(IHttpContextAccessor httpContextAccessor,
            LanguageService languageService,
            ILogger<NavBannerService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IConfiguration config,
            INavBannerImageRepository navBannerImageRepository,
            INavBannerLinkRepository navBannerLinkRepository,
            INavBannerLinkTextRepository navBannerLinkTextRepository,
            IPathResolverService pathResolverService) : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(httpContextAccessor);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(config);
            ArgumentNullException.ThrowIfNull(navBannerImageRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkTextRepository);
            ArgumentNullException.ThrowIfNull(pathResolverService);

            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _cache = cache;
            _config = config;
            _navBannerImageRepository = navBannerImageRepository;
            _navBannerLinkRepository = navBannerLinkRepository;
            _navBannerLinkTextRepository = navBannerLinkTextRepository;
            _pathResolverService = pathResolverService;
        }

        public async Task<NavBanner> GetByIdAsync(int navBannerId, bool forceReload)
        {
            var navBanner = new NavBanner
            {
                Id = navBannerId,
                NavBannerImage = await GetFromCacheDatabaseAsync(
                    Utility.Keys.Cache.PromNavBannerImage,
                    navBannerId,
                    await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService),
                    GetPageCacheDuration(_config),
                    _cache,
                    forceReload,
                    _navBannerImageRepository.GetByNavBannerIdAsync)
            };

            if (navBanner.NavBannerImage == null)
            {
                _logger.LogError("No NavBannerImage found for NavBanner id {Id}",
                    navBannerId);
                return null;
            }

            var navBannerImageLanguageName = await _languageService
                .GetLanguageNameAsync(navBanner.NavBannerImage.LanguageId, forceReload);

            navBanner.NavBannerImage.ImageLinkPath = _pathResolverService
                .GetPublicContentLink(ImagesFilePath,
                    navBannerImageLanguageName,
                    NavBannerFilePath,
                    navBanner.NavBannerImage.Filename);

            string navBannerLinksCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavBannerLinks,
                navBannerId);

            navBanner.NavBannerLinks = await _cache
                .GetObjectFromCacheAsync<ICollection<NavBannerLink>>(navBannerLinksCacheKey);

            if (navBanner.NavBannerLinks == null)
            {
                navBanner.NavBannerLinks = await _navBannerLinkRepository
                    .GetByNavBannerIdAsync(navBannerId);

                if (!navBanner.NavBannerLinks.Any())
                {
                    _logger.LogError("No NavBannerLinks found for NavBanner id {Id}",
                        navBannerId);
                    return null;
                }

                await _cache.SaveToCacheAsync(navBannerLinksCacheKey,
                    navBanner.NavBannerLinks,
                    GetPageCacheDuration(_config));
            }

            foreach (var navBannerLink in navBanner.NavBannerLinks)
            {
                var navBannerLinkText = await GetFromCacheDatabaseAsync(
                    Utility.Keys.Cache.PromNavBannerLinkText,
                    navBannerLink.Id,
                    await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService),
                    GetPageCacheDuration(_config),
                    _cache,
                    forceReload,
                    _navBannerLinkTextRepository.GetByLinkIdAsync);

                navBannerLink.LocalizedText = navBannerLinkText?.Text;
            }

            return navBanner;
        }
    }
}