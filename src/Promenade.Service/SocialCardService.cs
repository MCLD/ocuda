using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class SocialCardService : BaseService<SocialCardService>
    {
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly SiteSettingService _siteSettingService;
        private readonly ISocialCardRepository _socialCardRepository;

        public SocialCardService(ILogger<SocialCardService> logger,
            IDateTimeProvider dateTimeProvider,
            IConfiguration config,
            IDistributedCache cache,
            ISocialCardRepository socialCardRepository,
            SiteSettingService siteSettingService)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _socialCardRepository = socialCardRepository
                ?? throw new ArgumentNullException(nameof(socialCardRepository));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task<SocialCard> GetByIdAsync(int id, bool forceReload)
        {
            SocialCard card = null;

            var cachePagesInHours = GetPageCacheDuration(_config);
            string cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromSocialCard,
                id);

            if (cachePagesInHours > 0 && !forceReload)
            {
                card = await GetFromCacheAsync<SocialCard>(_cache, cacheKey);
            }

            if (card == null)
            {
                card = await _socialCardRepository.FindAsync(id);

                card.TwitterSite = await _siteSettingService.GetSettingStringAsync(
                        Models.Keys.SiteSetting.Social.TwitterUsername);

                await SaveToCacheAsync(_cache,
                    cacheKey,
                    card,
                    cachePagesInHours);
            }

            return card;
        }
    }
}