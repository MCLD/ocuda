using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class VolunteerFormService : BaseService<VolunteerFormService>
    {
        private readonly ILocationFormRepository _locationFormRepository;
        private readonly SegmentService _segmentService;
        private readonly IVolunteerFormRepository _volunteerFormRepository;
        private readonly IVolunteerFormSubmissionRepository _volunteerFormSubmissionRepository;

        public VolunteerFormService(ILogger<VolunteerFormService> logger,
            IDateTimeProvider dateTimeProvider,
            ILocationFormRepository locationFormRepository,
            IVolunteerFormRepository volunteerFormRepository,
            IVolunteerFormSubmissionRepository volunteerFormSubmissionRepository,
            SegmentService segmentService)
            : base(logger, dateTimeProvider)
        {
            _locationFormRepository = locationFormRepository
                ?? throw new ArgumentNullException(nameof(locationFormRepository));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
            _volunteerFormRepository = volunteerFormRepository
                ?? throw new ArgumentNullException(nameof(volunteerFormRepository));
            _volunteerFormSubmissionRepository = volunteerFormSubmissionRepository
                ?? throw new ArgumentNullException(nameof(volunteerFormSubmissionRepository));
        }

        public async Task<LocationForm> FindLocationFormAsync(int formId, int locationId)
        {
            return await _locationFormRepository.FindAsync(formId, locationId);
        }

        public async Task<VolunteerForm> FindVolunteerFormAsync(VolunteerFormType type, 
            bool forceReload)
        {
            var form = await _volunteerFormRepository.FindByTypeAsync(type);
            if (form.HeaderSegmentId.HasValue)
            {
                form.HeaderSegment = await _segmentService
                    .GetSegmentTextBySegmentIdAsync(form.HeaderSegmentId.Value, forceReload);
            }
            return form;
        }

        public async Task SaveSubmissionAsync(VolunteerFormSubmission form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }
            form.CreatedAt = DateTime.Now;
            form.StaffNotifiedAt = new DateTime();

            await _volunteerFormSubmissionRepository.AddAsync(form);
            await _volunteerFormSubmissionRepository.SaveAsync();
        }
    }
}