using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Defaults;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class SiteSettingPromService 
        : BaseService<SiteSettingPromService>, ISiteSettingPromService
    {
        private readonly ISiteSettingPromRepository _siteSettingPromRepository;

        public SiteSettingPromService(ILogger<SiteSettingPromService> logger,
            IHttpContextAccessor httpContextAccessor,
            ISiteSettingPromRepository siteSettingPromRepository)
            : base(logger, httpContextAccessor)
        {
            _siteSettingPromRepository = siteSettingPromRepository
                ?? throw new ArgumentNullException(nameof(siteSettingPromRepository));
        }

        /// <summary>
        /// Ensure all site settings exist in the database.
        /// </summary>
        public async Task EnsureSiteSettingsExistAsync()
        {
            var settingsToAdd = new List<SiteSetting>();

            foreach (var defaultSetting in SiteSettings.Get)
            {
                var siteSetting = await _siteSettingPromRepository.FindAsync(defaultSetting.Id);
                if (siteSetting == null)
                {
                    settingsToAdd.Add(defaultSetting);
                }
            }

            if (settingsToAdd.Count > 0)
            {
                await _siteSettingPromRepository.AddRangeAsync(settingsToAdd);
                await _siteSettingPromRepository.SaveAsync();
            }
        }

        public async Task<ICollection<SiteSetting>> GetAllAsync()
        {
            return await _siteSettingPromRepository.ToListAsync(_ => _.Category, _ => _.Name);
        }

        public async Task<SiteSetting> UpdateAsync(string key, string value)
        {
            var currentSetting = await _siteSettingPromRepository.FindAsync(key);

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

            ValidateSiteSetting(currentSetting);

            _siteSettingPromRepository.Update(currentSetting);
            await _siteSettingPromRepository.SaveAsync();

            // TODO: Add Promenade cache clearing for the updated setting

            return currentSetting;
        }

        public void ValidateSiteSetting(SiteSetting siteSetting)
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
