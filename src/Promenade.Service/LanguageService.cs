﻿using System;
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

namespace Ocuda.Promenade.Service
{
    public class LanguageService : BaseService<LanguageService>
    {
        private readonly IOptions<RequestLocalizationOptions> _l10nOptions;
        private readonly ILanguageRepository _languageRepository;

        public LanguageService(ILogger<LanguageService> logger,
            IDateTimeProvider dateTimeProvider,
            IOptions<RequestLocalizationOptions> l10nOptions,
            ILanguageRepository languageRepository)
            : base(logger, dateTimeProvider)
        {
            _l10nOptions = l10nOptions ?? throw new ArgumentNullException(nameof(l10nOptions));
            _languageRepository = languageRepository
                ?? throw new ArgumentNullException(nameof(languageRepository));
        }

        public async Task SyncLanguagesAsync()
        {
            var siteCultures = _l10nOptions
                .Value
                .SupportedCultures;

            var databaseCultures = await _languageRepository.GetAllAsync();

            foreach (var dbCulture in databaseCultures)
            {
                var siteCulture = siteCultures.SingleOrDefault(_ => _.Name == dbCulture.Name);
                if (siteCulture == null && dbCulture.IsActive)
                {
                    // no longer active
                    _logger.LogInformation("Marking language {Name} inactive in the database.",
                        dbCulture.Name);
                    dbCulture.IsActive = false;
                    dbCulture.IsDefault = dbCulture.Name == Culture.DefaultName;

                    _languageRepository.Update(dbCulture);
                }
                else if (siteCulture != null && !dbCulture.IsActive)
                {
                    // valid but marked invalid in the database
                    _logger.LogInformation("Marking language {Name} as active in the database.",
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

            await _languageRepository.SaveAsync();
        }

        public async Task<int> GetDefaultLanguageIdAsync()
        {
            return await _languageRepository.GetDefaultLanguageId();
        }

        public async Task<int> GetLanguageIdAsync(string culture)
        {
            return await _languageRepository.GetLanguageId(culture);
        }
    }
}
