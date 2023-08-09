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
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _locationFormRepository = locationFormRepository
                ?? throw new ArgumentNullException(nameof(locationFormRepository));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
            _volunteerFormRepository = volunteerFormRepository
                ?? throw new ArgumentNullException(nameof(volunteerFormRepository));
            _volunteerFormSubmissionRepository = volunteerFormSubmissionRepository
                ?? throw new ArgumentNullException(nameof(volunteerFormSubmissionRepository));
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

            await _volunteerFormSubmissionRepository.AddAsync(form);
            await _volunteerFormSubmissionRepository.SaveAsync();
        }
    }
}