using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class SiteSettingService : ISiteSettingService
    {
        private readonly ILogger _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly ISiteSettingRepository _siteSettingRepository;

        private int CacheMinutes { get; set; }

        public SiteSettingService(ILogger<SiteSettingService> logger,
            IDistributedCache cache,
            IConfiguration config,
            ISiteSettingRepository siteSettingRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));

            string timeoutSetting
                = _config[Utility.Keys.Configuration.OpsSiteSettingCacheMinutes] ?? "60";
            int.TryParse(timeoutSetting, out int timeout);
            CacheMinutes = timeout;
        }

        public async Task<ICollection<SiteSetting>> GetAllAsync()
        {
            return await _siteSettingRepository.ToListAsync(_ => _.Name);
        }

        public async Task<string> GetSetting(string key)
        {
            if (CacheMinutes == 0)
            {
                var siteSetting = await LookupSettingAsync(key);
                return siteSetting.Value;
            }
            else
            {
                string cacheKey = string.Format(Utility.Keys.Cache.OpsSiteSetting, key);
                var value = await _cache.GetStringAsync(key);
                if (value == null)
                {
                    var siteSetting = await LookupSettingAsync(key);
                    value = siteSetting.Value;
                    await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                            .SetAbsoluteExpiration(new TimeSpan(0, CacheMinutes, 0)));
                }
                return value;
            }
        }

        private async Task<SiteSetting> LookupSettingAsync(string key)
        {
            return await _siteSettingRepository.FindByKeyAsync(key);
        }

        public async Task<SiteSetting> UpdateAsync(string key, string value)
        {
            var currentSetting = await _siteSettingRepository.FindByKeyAsync(key);
            currentSetting.Value = value;

            _siteSettingRepository.Update(currentSetting);
            await _siteSettingRepository.SaveAsync();

            if (CacheMinutes > 0)
            {
                string cacheKey = string.Format(Utility.Keys.Cache.OpsSiteSetting, key);
                await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                    .SetAbsoluteExpiration(new TimeSpan(0, CacheMinutes, 0)));
            }

            return currentSetting;
        }
    }
}
