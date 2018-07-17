using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Defaults;
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

        /// <summary>
        /// Ensure all site settings exist in the database.
        /// </summary>
        public async Task EnsureSiteSettingsExistAsync(int sysadminId)
        {
            var settingsToAdd = new List<SiteSetting>();

            var defaultSiteSettings = DefaultSiteSettings.SiteSettings;
            foreach (var defaultSetting in defaultSiteSettings)
            {
                var siteSetting = await _siteSettingRepository.FindByKeyAsync(defaultSetting.Key);
                if (siteSetting == null)
                {
                    defaultSetting.CreatedAt = DateTime.Now;
                    defaultSetting.CreatedBy = sysadminId;
                    settingsToAdd.Add(defaultSetting);
                }
            }

            if (settingsToAdd.Count > 0)
            {
                await _siteSettingRepository.AddRangeAsync(settingsToAdd);
                await _siteSettingRepository.SaveAsync();
            }
        }

        public async Task<ICollection<SiteSetting>> GetAllAsync()
        {
            return await _siteSettingRepository.ToListAsync(_ => _.Name);
        }

        public async Task<bool> GetSettingBoolAsync(string key)
        {
            var settingValue = await GetSettingValueAsync(key);

            if (bool.TryParse(settingValue, out bool result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Invalid value for boolean setting {key}: {settingValue}");
            }
        }

        public async Task<int> GetSettingIntAsync(string key)
        {
            var settingValue = await GetSettingValueAsync(key);

            if (int.TryParse(settingValue, out int result))
            {
                return result;
            }
            else
            {
                throw new Exception($"Invalid value for integer setting {key}: {settingValue}");
            }
        }

        public async Task<string> GetSettingStringAsync(string key)
        {
            return await GetSettingValueAsync(key);
        }

        private async Task<string> GetSettingValueAsync(string key)
        {
            if (CacheMinutes == 0)
            {
                var siteSetting = await _siteSettingRepository.FindByKeyAsync(key);
                return siteSetting.Value;
            }
            else
            {
                string cacheKey = string.Format(Utility.Keys.Cache.OpsSiteSetting, key);
                var value = await _cache.GetStringAsync(key);
                if (value == null)
                {
                    var siteSetting = await _siteSettingRepository.FindByKeyAsync(key);
                    value = siteSetting.Value;
                    await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                            .SetAbsoluteExpiration(new TimeSpan(0, CacheMinutes, 0)));
                }
                return value;
            }
        }

        public async Task<SiteSetting> UpdateAsync(string key, string value)
        {
            var currentSetting = await _siteSettingRepository.FindByKeyAsync(key);

            if (currentSetting.Type == SiteSettingType.Bool)
            {
                if (!bool.TryParse(value, out bool result))
                {
                    _logger.LogError($"Invalid format for boolean key {key}: {value}");
                    throw new Exception("Invald format.");
                }
            }
            else if (currentSetting.Type == SiteSettingType.Int)
            {
                if (!int.TryParse(value, out int result))
                {
                    _logger.LogError($"Invalid format for integer key {key}: {value}");
                    throw new Exception("Invald format.");
                }
            }

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
