using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class NavigationService : BaseService<NavigationService>
    {
        private readonly IDistributedCache _cache;
        private readonly LanguageService _languageService;
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;

        public NavigationService(ILogger<NavigationService> logger,
            IDateTimeProvider dateTimeProvider,
            IDistributedCache cache,
            LanguageService languageService,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository) : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _navigationRepository = navigationRepository
                ?? throw new ArgumentNullException(nameof(navigationRepository));
            _navigationTextRepository = navigationTextRepository
                ?? throw new ArgumentNullException(nameof(navigationTextRepository));
        }

        public async Task<Navigation> GetNavigation(int navigationId)
        {
            return await GetNavigation(navigationId, false);
        }

        public async Task<Navigation> GetNavigation(int navigationId, bool forceReload)
        {
            long start = Stopwatch.GetTimestamp();
            var defaultLanguageId = await _languageService.GetDefaultLanguageIdAsync();

            Navigation nav = null;

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromNavLang,
                navigationId,
                defaultLanguageId);

            if (!forceReload)
            {
                string cachedNav = await _cache.GetStringAsync(cacheKey);

                if (!string.IsNullOrEmpty(cachedNav))
                {
                    try
                    {
                        nav = JsonSerializer.Deserialize<Navigation>(cachedNav);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex,
                            "Error deserializing navigation {NavigationId} language {LanguageId} from cache: {ErrorMessage}",
                            navigationId,
                            defaultLanguageId,
                            ex.Message);
                    }
                }
            }

            if (nav == null)
            {
                nav = await _navigationRepository.FindAsync(navigationId);
                if (nav.NavigationTextId != null)
                {
                    nav.NavigationText = await _navigationTextRepository
                        .FindAsync((int)nav.NavigationTextId, defaultLanguageId);
                }
                nav.Navigations = await GetNavigationChildren(navigationId, defaultLanguageId);

                string navToCache = JsonSerializer.Serialize(nav);
                await _cache.SetStringAsync(cacheKey,
                    navToCache,
                    new DistributedCacheEntryOptions
                    {
                        SlidingExpiration = new TimeSpan(1, 0, 0)
                    });
                _logger.LogDebug("Cache miss for {CacheKey}, caching {Length} characters in {Elapsed} ms",
                    cacheKey,
                    navToCache.Length,
                    (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
            }

            return nav;
        }

        private async Task<ICollection<Navigation>> GetNavigationChildren(int navigationId,
            int languageId)
        {
            var children = await _navigationRepository.GetChildren(navigationId);
            foreach (var child in children)
            {
                child.Navigations = await GetNavigationChildren(child.Id, languageId);
                if (child.NavigationTextId != null)
                {
                    child.NavigationText = await _navigationTextRepository
                        .FindAsync((int)child.NavigationTextId, languageId);
                }
            }
            return children;
        }
    }
}
