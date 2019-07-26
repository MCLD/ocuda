using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Promenade.Service
{
    public class SiteSettingService
    {
        private readonly ILogger _logger;
        private readonly ISiteSettingRepository _siteSettingRepository;

        public SiteSettingService(ILogger<SiteSettingService> logger,
            ISiteSettingRepository siteSettingRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _siteSettingRepository = siteSettingRepository
                ?? throw new ArgumentNullException(nameof(siteSettingRepository));
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
                throw new OcudaException($"Invalid value for promenade boolean setting {key}: {settingValue}");
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
                throw new OcudaException($"Invalid value for promenade integer setting {key}: {settingValue}");
            }
        }

        public async Task<string> GetSettingStringAsync(string key)
        {
            return await GetSettingValueAsync(key);
        }

        private async Task<string> GetSettingValueAsync(string key)
        {
            var siteSetting = await _siteSettingRepository.FindByKeyAsync(key);
            if (siteSetting == null)
            {
                _logger.LogError($"Promenade site setting key \"{key}\" not found.");
                siteSetting = Models.Defaults.SiteSettings.Get.Where(_ => _.Key == key).Single();
            }


            return siteSetting.Value;
        }
    }
}
