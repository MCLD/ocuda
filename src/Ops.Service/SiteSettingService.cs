using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Defaults;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class SiteSettingService : BaseService<SiteSettingService>, ISiteSettingService
    {
        private readonly IDistributedCache _cache;
        private readonly ISiteSettingRepository _siteSettingRepository;

        private int CacheMinutes { get; }

        public SiteSettingService(ILogger<SiteSettingService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache,
            IConfiguration config,
            ISiteSettingRepository siteSettingRepository)
            : base(logger, httpContextAccessor)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));

            string timeoutSetting
                = config[Utility.Keys.Configuration.OpsSiteSettingCacheMinutes] ?? "60";
            if (int.TryParse(timeoutSetting, out int timeout))
            {
                CacheMinutes = timeout;
            }
        }

        /// <summary>
        /// Ensure all site settings exist in the database.
        /// </summary>
        public async Task EnsureSiteSettingsExistAsync(int sysadminId)
        {
            var settingsToAdd = new List<SiteSetting>();

            foreach (var defaultSetting in SiteSettings.Get)
            {
                var siteSetting = await _siteSettingRepository.FindAsync(defaultSetting.Id);
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
            return await _siteSettingRepository.ToListAsync(_ => _.Category, _ => _.Name);
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
                throw new OcudaException($"Invalid value for boolean setting {key}: {settingValue}");
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
                throw new OcudaException($"Invalid value for integer setting {key}: {settingValue}");
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
                var siteSetting = await _siteSettingRepository.FindAsync(key);
                return siteSetting.Value;
            }
            else
            {
                string cacheKey = string.Format(Utility.Keys.Cache.OpsSiteSetting, key);
                var value = await _cache.GetStringAsync(key);
                if (value == null)
                {
                    var siteSetting = await _siteSettingRepository.FindAsync(key);
                    value = siteSetting.Value;
                    await _cache.SetStringAsync(cacheKey, value, new DistributedCacheEntryOptions()
                            .SetAbsoluteExpiration(new TimeSpan(0, CacheMinutes, 0)));
                }
                return value;
            }
        }

        public async Task<SiteSetting> UpdateAsync(string key, string value)
        {
            var currentSetting = await _siteSettingRepository.FindAsync(key);

            if (currentSetting.Type == SiteSettingType.Bool)
            {
                if (!bool.TryParse(value, out _))
                {
                    _logger.LogError("Invalid format for boolean site setting key {SiteSettingKey}: {SiteSettingValue}",
                        key,
                        value);
                    throw new OcudaException("Invald format.");
                }
            }
            else if (currentSetting.Type == SiteSettingType.Int && !int.TryParse(value, out _))
            {
                _logger.LogError("Invalid format for integer site setting key {SiteSettingKey}: {SiteSettingValue}",
                    key,
                    value);
                throw new OcudaException("Invald format.");
            }

            currentSetting.Value = value;
            currentSetting.UpdatedAt = DateTime.Now;
            currentSetting.UpdatedBy = GetCurrentUserId();

            ValidateSiteSettingAsync(currentSetting);

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

        public void ValidateSiteSettingAsync(SiteSetting siteSetting)
        {
            if (siteSetting == null)
            {
                throw new ArgumentNullException(nameof(siteSetting));
            }

            if (siteSetting.Type == SiteSettingType.Bool)
            {
                if (!bool.TryParse(siteSetting.Value, out bool result))
                {
                    _logger.LogWarning("{SiteSettingName} requires a value of type {SiteSettingType}",
                        siteSetting.Name,
                        siteSetting.Type);
                    throw new OcudaException($"{siteSetting.Name} requires a value of type {siteSetting.Type}.");
                }
            }
            else if (siteSetting.Type == SiteSettingType.Int
                && !int.TryParse(siteSetting.Value, out int result))
            {
                _logger.LogWarning("{SiteSettingName} requires a value of type {SiteSettingType}",
                    siteSetting.Name,
                    siteSetting.Type);
                throw new OcudaException($"{siteSetting.Name} requires a value of type {siteSetting.Type}.");
            }
        }
    }
}
