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
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class IncidentService : BaseService<IncidentService>, IIncidentService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIncidentParticipantRepository _incidentParticipantRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IIncidentTypeRepository _incidentTypeRepository;
        private readonly IIncidentStaffRepository _incidentStaffRepository;
        private readonly ILocationRepository _locationRepository;

        public IncidentService(ILogger<IncidentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IIncidentParticipantRepository incidentParticipantRepository,
            IIncidentRepository incidentRepository,
            IIncidentTypeRepository incidentTypeRepository,
            IIncidentStaffRepository incidentStaffRepository,
            ILocationRepository locationRepository) : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _incidentParticipantRepository = incidentParticipantRepository
                ?? throw new ArgumentNullException(nameof(incidentParticipantRepository));
            _incidentRepository = incidentRepository
                ?? throw new ArgumentNullException(nameof(incidentRepository));
            _incidentTypeRepository = incidentTypeRepository
                ?? throw new ArgumentNullException(nameof(incidentTypeRepository));
            _incidentStaffRepository = incidentStaffRepository
                ?? throw new ArgumentNullException(nameof(incidentStaffRepository));
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
        }

        public async Task<int> AddAsync(Incident incident,
            ICollection<IncidentStaff> staffs,
            ICollection<IncidentParticipant> participants)
        {
            if (incident == null) { throw new ArgumentNullException(nameof(incident)); }
            if (staffs == null) { throw new ArgumentNullException(nameof(staffs)); }
            if (participants == null) { throw new ArgumentNullException(nameof(participants)); }

            var now = _dateTimeProvider.Now;

            incident.CreatedAt = now;
            incident.CreatedBy = GetCurrentUserId();
            await _incidentRepository.AddAsync(incident);
            await _incidentRepository.SaveAsync();

            if (staffs.Count > 0)
            {
                foreach (var staff in staffs)
                {
                    staff.CreatedAt = now;
                    staff.CreatedBy = GetCurrentUserId();
                    staff.IncidentId = incident.Id;
                }
                await _incidentStaffRepository.AddRangeAsync(staffs);
                await _incidentStaffRepository.SaveAsync();
            }

            if (participants.Count > 0)
            {
                foreach (var participant in participants)
                {
                    participant.CreatedAt = now;
                    participant.CreatedBy = GetCurrentUserId();
                    participant.IncidentId = incident.Id;
                }

                await _incidentParticipantRepository.AddRangeAsync(participants);
                await _incidentParticipantRepository.SaveAsync();
            }

            return incident.Id;
        }

        public async Task AddTypeAsync(string incidentTypeName)
        {
            await _incidentTypeRepository.AddAsync(new IncidentType
            {
                CreatedAt = _dateTimeProvider.Now,
                CreatedBy = GetCurrentUserId(),
                Description = incidentTypeName,
                IsActive = true
            });
            await _incidentTypeRepository.SaveAsync();
        }

        public async Task AdjustTypeStatus(int incidentTypeId, bool status)
        {
            var type = await _incidentTypeRepository.FindAsync(incidentTypeId);
            type.IsActive = status;
            type.UpdatedAt = _dateTimeProvider.Now;
            type.UpdatedBy = GetCurrentUserId();
            _incidentTypeRepository.Update(type);
            await _incidentTypeRepository.SaveAsync();
        }

        public async Task<IDictionary<int, string>> GetActiveIncidentTypesAsync()
        {
            var incidentTypes = await _incidentTypeRepository.GetActiveAsync();
            return incidentTypes.ToDictionary(k => k.Id, v => v.Description);
        }

        public async Task<Dictionary<int, string>> GetAllIncidentTypesAsync()
        {
            var incidentTypes = await _incidentTypeRepository.GetAllAsync();
            return incidentTypes.ToDictionary(k => k.Id, v => v.Description);
        }

        public async Task<CollectionWithCount<IncidentType>> GetIncidentTypesAsync(BaseFilter filter)
        {
            return await _incidentTypeRepository.GetAsync(filter);
        }

        public async Task<CollectionWithCount<Incident>> GetPaginatedAsync(IncidentFilter filter)
        {
            if (filter == null) { filter = new IncidentFilter(); }

            if (!string.IsNullOrEmpty(filter.SearchText))
            {
                filter.LocationIds
                    = await _locationRepository.SearchIdsByNameAsync(filter.SearchText);
            }

            return await _incidentRepository.GetPaginatedAsync(filter);
        }

        public async Task<IncidentType> GetTypeAsync(string incidentTypeName)
        {
            return await _incidentTypeRepository.GetAsync(incidentTypeName);
        }

        public async Task UpdateIncidentType(int incidentTypeId, string incidentTypeDescription)
        {
            var type = await _incidentTypeRepository.FindAsync(incidentTypeId);
            if (type == null)
            {
                throw new Utility.Exceptions.OcudaException("Incident type not found.");
            }
            type.Description = incidentTypeDescription;
            type.UpdatedAt = _dateTimeProvider.Now;
            type.UpdatedBy = GetCurrentUserId();
            _incidentTypeRepository.Update(type);
            await _incidentTypeRepository.SaveAsync();
        }
    }
}
