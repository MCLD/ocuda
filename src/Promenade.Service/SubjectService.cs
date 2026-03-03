using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class SubjectService(ILogger<SubjectService> logger,
        IDateTimeProvider dateTimeProvider,
        IConfiguration config,
        IHttpContextAccessor httpContextAccessor,
        IOcudaCache cache,
        ISubjectRepository subjectRepository,
        ISubjectTextRepository subjectTextRepository,
        LanguageService languageService)
            : BaseService<SubjectService>(logger, dateTimeProvider)
    {
        private readonly IOcudaCache _cache = cache
            ?? throw new ArgumentNullException(nameof(cache));

        private readonly IConfiguration _config = config
            ?? throw new ArgumentNullException(nameof(config));

        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        private readonly LanguageService _languageService = languageService
            ?? throw new ArgumentNullException(nameof(languageService));

        private readonly ISubjectRepository _subjectRepository = subjectRepository
            ?? throw new ArgumentNullException(nameof(subjectRepository));

        private readonly ISubjectTextRepository _subjectTextRepository = subjectTextRepository
            ?? throw new ArgumentNullException(nameof(subjectTextRepository));

        private int CachePagesInHours
        {
            get
            {
                return GetPageCacheDuration(_config);
            }
        }

        public async Task<ICollection<Subject>> GetAllAsync(bool forceReload)
        {
            ICollection<Subject> subjects = null;

            if (CachePagesInHours > 0 && !forceReload)
            {
                subjects = await _cache
                    .GetObjectFromCacheAsync<ICollection<Subject>>(Utility.Keys.Cache.PromSubjects);
            }

            if (subjects == null || subjects.Count == 0)
            {
                subjects = await _subjectRepository.GetAllAsync();

                await _cache.SaveToCacheAsync(Utility.Keys.Cache.PromSubjects,
                    subjects,
                    CachePagesInHours);
            }
            return subjects;
        }

        public async Task<IDictionary<string, string>> GetSlugsDescriptionsAsync(bool forceReload)
        {
            IDictionary<string, string> slugsDescriptions = null;

            var langIds
                = await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService);

            var cacheKey = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromSubjectSlugsDesc,
                langIds[0]);

            if (CachePagesInHours > 0 && !forceReload)
            {
                slugsDescriptions = await _cache
                    .GetObjectFromCacheAsync<IDictionary<string, string>>(cacheKey);
            }

            if (slugsDescriptions == null || slugsDescriptions.Count == 0)
            {
                var subjects = await _subjectRepository.GetAllAsync();

                slugsDescriptions = new Dictionary<string, string>();

                foreach (var subject in subjects)
                {
                    foreach (var langId in langIds)
                    {
                        var description = await _subjectTextRepository
                            .GetByIdsAsync(subject.Id, langId);
                        if (description != null)
                        {
                            slugsDescriptions.Add(subject.Slug, description.Text);
                            break;
                        }
                    }
                }

                await _cache.SaveToCacheAsync(cacheKey, slugsDescriptions, CachePagesInHours);
            }

            return slugsDescriptions;
        }

        public async Task<SubjectText> GetTextAsync(bool forceReload, int subjectId)
        {
            return await GetFromCacheDatabaseAsync(Utility.Keys.Cache.PromSubjectText,
                subjectId,
                await GetCurrentDefaultLanguageIdAsync(_httpContextAccessor, _languageService),
                CachePagesInHours,
                _cache,
                forceReload,
                _subjectTextRepository.GetByIdsAsync);
        }
    }
}