using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class SiteSettingService : BaseService<SiteSettingService>
    {
        private const string NoValue = "null";

        private readonly IOcudaCache _cache;
        private readonly ISiteSettingRepository _siteSettingRepository;

        public SiteSettingService(ILogger<SiteSettingService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            ISiteSettingRepository siteSettingRepository) : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(logger));
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));
        }

        public async Task<bool> GetSettingBoolAsync(string key)
        {
            return await GetSettingBoolAsync(key, false);
        }

        public async Task<bool> GetSettingBoolAsync(string key, bool forceReload)
        {
            var settingValue = await GetSettingValueAsync(key, forceReload);

            if (bool.TryParse(settingValue, out bool result))
            {
                return result;
            }
            else
            {
                _logger.LogError("Invalid value for Promenade boolean setting {SiteSettingKey}: {SiteSettingValue}",
                    key,
                    settingValue);

                var defaultSetting = GetDefaultSetting(key);
                if (bool.TryParse(defaultSetting.Value, out bool defaultResult))
                {
                    return defaultResult;
                }
                else
                {
                    _logger.LogCritical("Invalid default value for Promenade boolean setting {SiteSettingKey}: {SiteSettingValue}",
                        key,
                        settingValue);
                    return default;
                }
            }
        }

        public async Task<double> GetSettingDoubleAsync(string key)
        {
            return await GetSettingDoubleAsync(key, false);
        }

        public async Task<double> GetSettingDoubleAsync(string key, bool forceReload)
        {
            var settingValue = await GetSettingValueAsync(key, forceReload);

            if (double.TryParse(settingValue, out double result))
            {
                return result;
            }
            else
            {
                if (!string.IsNullOrEmpty(settingValue))
                {
                    _logger.LogError("Invalid value for Promenade double setting {SiteSettingKey}: {SiteSettingValue}",
                        key,
                        settingValue);
                }

                var defaultSetting = GetDefaultSetting(key);
                if (double.TryParse(defaultSetting.Value, out double defaultResult))
                {
                    return defaultResult;
                }
                else
                {
                    if (!string.IsNullOrEmpty(defaultSetting.Value))
                    {
                        _logger.LogCritical("Invalid default value for Promenade double setting {SiteSettingKey}: {SiteSettingValue}",
                            key,
                            settingValue);
                    }
                    return default;
                }
            }
        }

        public async Task<int> GetSettingIntAsync(string key)
        {
            return await GetSettingIntAsync(key, false);
        }

        public async Task<int> GetSettingIntAsync(string key, bool forceReload)
        {
            var settingValue = await GetSettingValueAsync(key, forceReload);

            if (int.TryParse(settingValue, out int result))
            {
                return result;
            }
            else
            {
                if (!string.IsNullOrEmpty(settingValue))
                {
                    _logger.LogError("Invalid value for Promenade integer setting {SiteSettingKey}: {SiteSettingValue}",
                        key,
                        settingValue);
                }

                var defaultSetting = GetDefaultSetting(key);
                if (int.TryParse(defaultSetting.Value, out int defaultResult))
                {
                    return defaultResult;
                }
                else
                {
                    if (!string.IsNullOrEmpty(defaultSetting.Value))
                    {
                        _logger.LogCritical("Invalid default value for Promenade integer setting {SiteSettingKey}: {SiteSettingValue}",
                            key,
                            settingValue);
                    }
                    return default;
                }
            }
        }

        public async Task<string> GetSettingStringAsync(string key)
        {
            return await GetSettingStringAsync(key, false);
        }

        public async Task<string> GetSettingStringAsync(string key, bool forceReload)
        {
            return await GetSettingValueAsync(key, forceReload);
        }

        private static SiteSetting GetDefaultSetting(string key)
            => Models.Defaults.SiteSettings.Get.Single(_ => _.Id == key);

        private async Task<string> GetSettingValueAsync(string key, bool forceReload)
        {
            string setting = null;

            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromSiteSetting,
                key);

            if (!forceReload)
            {
                setting = await _cache.GetStringFromCache(cacheKey);
                if (setting?.Equals(NoValue, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return null;
                }
            }

            if (string.IsNullOrEmpty(setting))
            {
                var siteSetting = await _siteSettingRepository.FindAsync(key)
                    ?? GetDefaultSetting(key);
                setting = siteSetting?.Value ?? NoValue;
                await _cache.SaveToCacheAsync(cacheKey,
                    setting,
                    TimeSpan.FromHours(12),
                    CacheSlidingExpiration);
            }
            return setting;
        }
    }
}