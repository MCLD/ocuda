using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocuda.i18n;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class LanguageService : BaseService<LanguageService>
    {
        private readonly IOcudaCache _cache;
        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;
        private readonly ILanguageRepository _languageRepository;

        public LanguageService(ILogger<LanguageService> logger,
            IDateTimeProvider dateTimeProvider,
            IOcudaCache cache,
            IOptions<RequestLocalizationOptions> l10nOptions,
            ILanguageRepository languageRepository)
            : base(logger, dateTimeProvider)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _l10nOptions = l10nOptions ?? throw new ArgumentNullException(nameof(l10nOptions));
            _languageRepository = languageRepository
                ?? throw new ArgumentNullException(nameof(languageRepository));
        }

        public async Task<int> GetDefaultLanguageIdAsync()
        {
            return await GetDefaultLanguageIdAsync(false);
        }

        public async Task<int> GetDefaultLanguageIdAsync(bool forceReload)
        {
            var cacheKey = Utility.Keys.Cache.PromDefaultLanguageId;

            if (!forceReload)
            {
                var cachedLanguageId = await _cache.GetIntFromCacheAsync(cacheKey);
                if (cachedLanguageId.HasValue)
                {
                    return cachedLanguageId.Value;
                }
            }

            int languageId = await _languageRepository.GetDefaultLanguageId();

            await _cache.SaveToCacheAsync(cacheKey,
                languageId,
                TimeSpan.FromHours(12),
                CacheSlidingExpiration);

            return languageId;
        }

        public async Task<int> GetLanguageIdAsync(string culture)
        {
            return await GetLanguageIdAsync(culture, false);
        }

        public async Task<int> GetLanguageIdAsync(string culture, bool forceReload)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLanguageId,
                culture);

            if (!forceReload)
            {
                var cachedLanguageId = await _cache.GetIntFromCacheAsync(cacheKey);

                if (cachedLanguageId.HasValue)
                {
                    return cachedLanguageId.Value;
                }
            }

            int languageId = await _languageRepository.GetLanguageId(culture);

            await _cache.SaveToCacheAsync(cacheKey,
                languageId,
                TimeSpan.FromHours(12),
                CacheSlidingExpiration);

            return languageId;
        }

        public async Task<string> GetNameAsync(int id, bool forceReload)
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromLanguageName,
                id);

            if (!forceReload)
            {
                var cachedLanguageName = await _cache.GetStringFromCache(cacheKey);

                if (!string.IsNullOrEmpty(cachedLanguageName))
                {
                    return cachedLanguageName;
                }
            }

            var language = await _languageRepository.GetActiveByIdAsync(id);

            if (language != null)
            {
                await _cache.SaveToCacheAsync(cacheKey,
                    language.Name,
                    TimeSpan.FromHours(12),
                    CacheSlidingExpiration);
            }

            return language?.Name;
        }

        public async Task SyncLanguagesAsync()
        {
            var siteCultures = _l10nOptions
                .Value
                .SupportedCultures;

            var databaseCultures = await _languageRepository.GetAllAsync();

            _logger.LogDebug("Removing {DefaultLanguageCacheKey} from cache on startup",
                Utility.Keys.Cache.PromDefaultLanguageId);
            await _cache.RemoveAsync(Utility.Keys.Cache.PromDefaultLanguageId);

            foreach (var dbCulture in databaseCultures)
            {
                var siteCulture = siteCultures.SingleOrDefault(_ => _.Name == dbCulture.Name);
                if (siteCulture == null && dbCulture.IsActive)
                {
                    // no longer active
                    _logger.LogInformation("Marking language {LanguageName} inactive in the database.",
                        dbCulture.Name);
                    dbCulture.IsActive = false;
                    dbCulture.IsDefault = dbCulture.Name == Culture.DefaultName;

                    _languageRepository.Update(dbCulture);
                }
                else if (siteCulture != null && !dbCulture.IsActive)
                {
                    // valid but marked invalid in the database
                    _logger.LogInformation("Marking language {LanguageName} as active in the database.",
                        dbCulture.Name);
                    dbCulture.IsActive = true;
                    dbCulture.IsDefault = dbCulture.Name == Culture.DefaultName;

                    _languageRepository.Update(dbCulture);
                }
                else
                {
                    bool doUpdate = false;
                    // ensure default is set properly
                    if ((dbCulture.IsDefault && dbCulture.Name != Culture.DefaultName)
                        || (!dbCulture.IsDefault && dbCulture.Name == Culture.DefaultName))
                    {
                        dbCulture.IsDefault = dbCulture.Name == Culture.DefaultName;
                        doUpdate = true;
                    }
                    if (dbCulture.IsDefault
                        && dbCulture.Description != Culture.DefaultCulture.DisplayName)
                    {
                        dbCulture.Description = Culture.DefaultCulture.DisplayName;
                        doUpdate = true;
                    }
                    if (doUpdate)
                    {
                        _languageRepository.Update(dbCulture);
                    }
                }

                var langNameCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromLanguageName,
                    dbCulture.Id);
                var langIdCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromLanguageId,
                    dbCulture.Name);

                _logger.LogDebug("Removing {LanguageNameCacheKey} and {LanguageIdCacheKey} from cache on startup",
                    langNameCacheKey,
                    langIdCacheKey);

                await _cache.RemoveAsync(langNameCacheKey);
                await _cache.RemoveAsync(langIdCacheKey);
            }

            var namesMissingFromDb = siteCultures
                .Select(_ => _.Name)
                .Except(databaseCultures.Select(_ => _.Name));

            foreach (var missingCultureName in namesMissingFromDb)
            {
                var culture = siteCultures.Single(_ => _.Name == missingCultureName);
                await _languageRepository.Add(new Language
                {
                    Description = culture.DisplayName,
                    IsActive = true,
                    IsDefault = culture.Name == Culture.DefaultName,
                    Name = culture.Name
                });
            }

            foreach (var siteCulture in siteCultures)
            {
                var databaseCulture = databaseCultures
                    .SingleOrDefault(_ => _.Name == siteCulture.Name);

                if (databaseCulture != null
                    && databaseCulture.Description != siteCulture.DisplayName)
                {
                    _logger.LogInformation("Fixing description for culture {Culture} in database from {OldDescription} to {NewDescription}",
                        siteCulture.Name,
                        databaseCulture.Description,
                        siteCulture.Name);
                    databaseCulture.Description = siteCulture.DisplayName;
                    _languageRepository.Update(databaseCulture);
                }
            }

            await _languageRepository.SaveAsync();
        }
    }
}