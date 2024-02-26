using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Service
{
    public class VolunteerFormService : BaseService<VolunteerFormService>
    {
        private const int CacheFormHours = 1;
        private readonly IOcudaCache _cache;
        private readonly ILocationFormRepository _locationFormRepository;
        private readonly SegmentService _segmentService;
        private readonly IVolunteerFormRepository _volunteerFormRepository;
        private readonly IVolunteerFormSubmissionRepository _volunteerFormSubmissionRepository;

        public VolunteerFormService(ILogger<VolunteerFormService> logger,
            IDateTimeProvider dateTimeProvider,
            ILocationFormRepository locationFormRepository,
            IOcudaCache cache,
            IVolunteerFormRepository volunteerFormRepository,
            IVolunteerFormSubmissionRepository volunteerFormSubmissionRepository,
            SegmentService segmentService)
            : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(locationFormRepository);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(volunteerFormRepository);
            ArgumentNullException.ThrowIfNull(volunteerFormSubmissionRepository);

            _cache = cache;
            _locationFormRepository = locationFormRepository;
            _segmentService = segmentService;
            _volunteerFormRepository = volunteerFormRepository;
            _volunteerFormSubmissionRepository = volunteerFormSubmissionRepository;
        }

        public async Task<LocationForm> FindLocationFormAsync(VolunteerFormType type,
            int locationId,
            bool forceReload)
        {
            var formCacheKey = string.Format(CultureInfo.InvariantCulture,
                Utility.Keys.Cache.PromVolunteerForm,
                type,
                locationId);

            var form = forceReload
                ? null
                : await _cache.GetObjectFromCacheAsync<VolunteerForm>(formCacheKey);

            if (form == null)
            {
                form = await _volunteerFormRepository.FindByTypeAsync(type);

                if (form != null)
                {
                    await _cache.SaveToCacheAsync(formCacheKey, form, CacheFormHours);
                }
            }

            if (form?.IsDisabled == false)
            {
                if (form.HeaderSegmentId.HasValue)
                {
                    form.HeaderSegment = await _segmentService
                        .GetSegmentTextBySegmentIdAsync(form.HeaderSegmentId.Value, forceReload);
                }

                var formLocationCacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.PromVolunteerFormLocation,
                    form.Id,
                    locationId);

                var locationForm = forceReload
                    ? null
                    : await _cache.GetObjectFromCacheAsync<LocationForm>(formLocationCacheKey);

                if (locationForm == null)
                {
                    locationForm = await _locationFormRepository.FindAsync(form.Id, locationId);
                    if (locationForm != null)
                    {
                        await _cache
                            .SaveToCacheAsync(formLocationCacheKey, locationForm, CacheFormHours);
                    }
                }

                if (locationForm != null)
                {
                    locationForm.Form = form;
                    return locationForm;
                }
            }
            return null;
        }

        public async Task SaveSubmissionAsync(VolunteerFormSubmission form)
        {
            ArgumentNullException.ThrowIfNull(form);

            form.CreatedAt = _dateTimeProvider.Now;

            _logger.LogInformation("Saving {FormType} volunteer form submission from {Name}",
                form.VolunteerFormType,
                form.Name);

            await _volunteerFormSubmissionRepository.AddAsync(form);
            await _volunteerFormSubmissionRepository.SaveAsync();
        }
    }
}