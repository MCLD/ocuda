using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class VolunteerFormService : BaseService<VolunteerFormService>, IVolunteerFormService
    {
        private readonly ILocationFeatureRepository _locationFeatureRepository;
        private readonly ILocationFormRepository _locationFormRepository;
        private readonly IVolunteerFormRepository _volunteerFormRepository;
        private readonly IVolunteerFormSubmissionEmailRecordRepository _emailRecordRepository;
        private readonly IVolunteerFormSubmissionRepository _volunteerFormSubmissionRepository;
        private readonly IVolunteerUserMappingRepository _volunteerUserMappingRepository;

        public VolunteerFormService(IHttpContextAccessor httpContextAccessor,
            ILocationFeatureRepository locationFeatureRepository,
            ILocationFormRepository locationFormRepository,
            ILogger<VolunteerFormService> logger,
            IVolunteerFormRepository volunteerFormRepository,
            IVolunteerFormSubmissionEmailRecordRepository emailRecordRepository,
            IVolunteerFormSubmissionRepository volunteerFormSubmissionRepository,
            IVolunteerUserMappingRepository volunteerUserMappingRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(emailRecordRepository);
            ArgumentNullException.ThrowIfNull(locationFeatureRepository);
            ArgumentNullException.ThrowIfNull(locationFormRepository);
            ArgumentNullException.ThrowIfNull(volunteerFormRepository);
            ArgumentNullException.ThrowIfNull(volunteerFormSubmissionRepository);
            ArgumentNullException.ThrowIfNull(volunteerUserMappingRepository);

            _emailRecordRepository = emailRecordRepository;
            _locationFeatureRepository = locationFeatureRepository;
            _locationFormRepository = locationFormRepository;
            _volunteerFormRepository = volunteerFormRepository;
            _volunteerFormSubmissionRepository = volunteerFormSubmissionRepository;
            _volunteerUserMappingRepository = volunteerUserMappingRepository;
        }

        public async Task AddFormUserMapping(int locationId, VolunteerFormType type, int userId)
        {
            var form = await GetFormByTypeAsync(type);
            if (form != null)
            {
                var loc = await _volunteerUserMappingRepository
                    .GetByLocationFormAsync(locationId, form.Id);
                if (loc.Any(_ => _.UserId == userId))
                {
                    throw new OcudaException("Staff member already configured to receive those submissions.");
                }

                await _volunteerUserMappingRepository.AddSaveFormUserMappingAsync(form.Id,
                    locationId,
                    userId);
                var locationForm = await _locationFormRepository.FindAsync(locationId, form.Id);
                if (locationForm == null)
                {
                    await _locationFormRepository.AddSaveLocationForm(locationId, form.Id);
                }
            }
            else
            {
                throw new OcudaException($"{type} Volunteer form was not found.");
            }
        }

        public async Task<VolunteerForm> AddUpdateFormAsync(VolunteerForm form)
        {
            ArgumentNullException.ThrowIfNull(form);
            var existingForm = await _volunteerFormRepository
                .FindByTypeAsync(form.VolunteerFormType);
            if (existingForm != null)
            {
                if (existingForm.IsDisabled)
                {
                    existingForm.IsDisabled = false;
                    await _volunteerFormRepository.UpdateSaveAsync(existingForm);
                    return await _volunteerFormRepository
                        .FindByTypeAsync(existingForm.VolunteerFormType);
                }

                throw new OcudaException($"Form type '{form.VolunteerFormType}' already exists.");
            }
            else
            {
                await _volunteerFormRepository.AddSaveAsync(form);
            }

            return await _volunteerFormRepository.FindAsync(form.Id);
        }

        public async Task AddVolunteerLocationFeature(int featureId,
            int locationId,
            string locationStub)
        {
            await _locationFeatureRepository
                .AddAsync(new LocationFeature
                {
                    LocationId = locationId,
                    FeatureId = featureId,
                    RedirectUrl = $"/{locationStub}/volunteer"
                });
            await _locationFeatureRepository.SaveAsync();
        }

        public async Task DisableAsync(int formId)
        {
            await SetDisabled(formId, true);
        }

        public async Task<VolunteerForm> EditAsync(VolunteerForm form)
        {
            ArgumentNullException.ThrowIfNull(form);
            var currentForm = await _volunteerFormRepository.FindAsync(form.Id);

            if (currentForm != null)
            {
                currentForm.VolunteerFormType = form.VolunteerFormType;
                currentForm.HeaderSegmentId = form.HeaderSegmentId;
                await _volunteerFormRepository.UpdateSaveAsync(currentForm);
                return currentForm;
            }
            else
            {
                throw new OcudaException($"Could not find form id {form.Id} to edit.");
            }
        }

        public async Task EnableAsync(int formId)
        {
            await SetDisabled(formId, false);
        }

        public IDictionary<string, int> GetAllVolunteerFormTypes()
        {
            return Enum.GetValues(typeof(VolunteerFormType))
                .Cast<VolunteerFormType>()
                .ToDictionary(_ => _.ToString(), _ => (int)_);
        }

        public async Task<IDictionary<VolunteerFormType, int>> GetEmailSetupOverflowMappingAsync()
        {
            return await _volunteerFormRepository.GetEmailSetupOverflowMappingAsync();
        }

        public async Task<VolunteerForm> GetFormByIdAsync(int Id)
        {
            return await _volunteerFormRepository.FindAsync(Id);
        }

        public async Task<ICollection<VolunteerForm>> GetFormBySegmentIdAsync(int segmentId)
        {
            return await _volunteerFormRepository.FindBySegmentIdAsync(segmentId);
        }

        public async Task<VolunteerForm> GetFormByTypeAsync(VolunteerFormType type)
        {
            return await _volunteerFormRepository.FindByTypeAsync(type);
        }

        public async Task<IDictionary<VolunteerFormType, int>> GetFormEmailSetupMappingAsync()
        {
            return await _volunteerFormRepository.GetEmailSetupMappingAsync();
        }

        public async Task<ICollection<VolunteerFormUserMapping>>
            GetFormUserMappingsAsync(int formId, int locationId)
        {
            return await _volunteerUserMappingRepository.GetByLocationFormAsync(locationId, formId);
        }

        public async Task<ICollection<VolunteerFormSubmissionEmailRecord>>
            GetNotificationInfoAsync(int submissionId)
        {
            return await _emailRecordRepository.GetBySubmissionId(submissionId);
        }

        public async Task<DataWithCount<ICollection<VolunteerForm>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _volunteerFormRepository.GetPaginatedListAsync(filter);
        }

        public async Task<CollectionWithCount<VolunteerFormSubmission>>
            GetPaginatedSubmissionsAsync(VolunteerSubmissionFilter filter)
        {
            return await _volunteerFormSubmissionRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<VolunteerFormSubmission>> GetPendingNotificationsAsync()
        {
            return await _volunteerFormSubmissionRepository.GetPendingNotificationsAsync();
        }

        public async Task<ICollection<VolunteerFormUserMapping>> GetUserMappingsAsync(int userId)
        {
            return await _volunteerUserMappingRepository.GetByUserAsync(userId);
        }

        public async Task<ICollection<VolunteerForm>> GetVolunteerFormsAsync()
        {
            return await _volunteerFormRepository.FindAllAsync();
        }

        public async Task<VolunteerFormSubmission> GetVolunteerFormSubmissionAsync(int submissionId)
        {
            return await _volunteerFormSubmissionRepository
                .GetByIdAsync(submissionId);
        }

        public async Task<ICollection<VolunteerFormSubmission>>
            GetVolunteerFormSubmissionsAsync(int locationId, int typeId)
        {
            var form = await _volunteerFormRepository.FindByTypeAsync(typeId);
            return await _volunteerFormSubmissionRepository.GetAllAsync(locationId, form.Id);
        }

        public async Task NotifiedStaffAsync(int formSubmissionId, int? emailRecordId, int userId)
        {
            await _volunteerFormSubmissionRepository.StaffNotifiedAsync(formSubmissionId);
            if (emailRecordId.HasValue)
            {
                await _emailRecordRepository.AddSaveAsync(formSubmissionId,
                    emailRecordId.Value,
                    userId);
            }
        }

        public async Task RemoveFormUserMapping(int locationId, int userId, VolunteerFormType type)
        {
            var form = await GetFormByTypeAsync(type);
            if (form != null)
            {
                await _volunteerUserMappingRepository
                    .RemoveFormUserMappingAsync(form.Id, locationId, userId);

                var locationMappings = await _volunteerUserMappingRepository
                    .GetByLocationFormAsync(locationId, form.Id);
                var locationForm = await _locationFormRepository.FindAsync(locationId, form.Id);
                if (locationForm != null && locationMappings.Count == 0)
                {
                    await _locationFormRepository.RemoveSaveAsync(locationForm);
                }
            }
        }

        private async Task SetDisabled(int formId, bool isDisabled)
        {
            var form = await GetFormByIdAsync(formId)
                ?? throw new OcudaException($"Unable to find form id {formId}.");
            form.IsDisabled = isDisabled;
            await _volunteerFormRepository.UpdateSaveAsync(form);
        }
    }
}