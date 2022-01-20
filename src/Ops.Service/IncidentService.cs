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
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class IncidentService : BaseService<IncidentService>, IIncidentService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIncidentFollowupRepository _incidentFollowupRepository;
        private readonly IIncidentParticipantRepository _incidentParticipantRepository;
        private readonly IIncidentRelationshipRepository _incidentRelationshipRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IIncidentStaffRepository _incidentStaffRepository;
        private readonly IIncidentTypeRepository _incidentTypeRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IUserService _userService;

        public IncidentService(ILogger<IncidentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IIncidentFollowupRepository incidentFollowupRepository,
            IIncidentParticipantRepository incidentParticipantRepository,
            IIncidentRelationshipRepository incidentRelationshipRepository,
            IIncidentRepository incidentRepository,
            IIncidentTypeRepository incidentTypeRepository,
            IIncidentStaffRepository incidentStaffRepository,
            ILocationRepository locationRepository,
            IUserService userService) : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _incidentFollowupRepository = incidentFollowupRepository
                ?? throw new ArgumentNullException(nameof(incidentFollowupRepository));
            _incidentParticipantRepository = incidentParticipantRepository
                ?? throw new ArgumentNullException(nameof(incidentParticipantRepository));
            _incidentRelationshipRepository = incidentRelationshipRepository
                ?? throw new ArgumentNullException(nameof(incidentRelationshipRepository));
            _incidentRepository = incidentRepository
                ?? throw new ArgumentNullException(nameof(incidentRepository));
            _incidentStaffRepository = incidentStaffRepository
                ?? throw new ArgumentNullException(nameof(incidentStaffRepository));
            _incidentTypeRepository = incidentTypeRepository
                ?? throw new ArgumentNullException(nameof(incidentTypeRepository));
            _locationRepository = locationRepository
                ?? throw new ArgumentNullException(nameof(locationRepository));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
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

        public async Task AddFollowupAsync(int incidentId, string followupText)
        {
            await _incidentFollowupRepository.AddAsync(new IncidentFollowup
            {
                CreatedAt = _dateTimeProvider.Now,
                CreatedBy = GetCurrentUserId(),
                Description = followupText,
                IncidentId = incidentId
            });
            await _incidentFollowupRepository.SaveAsync();
        }

        public async Task AddRelationshipAsync(int incidentId, int relatedIncidentId)
        {
            if (incidentId == relatedIncidentId)
            {
                throw new OcudaException("Cannot relate an incident to itself.");
            }

            var forwardRelationship = await _incidentRelationshipRepository
                .GetByIncidentIdAsync(incidentId);

            var forwardHasRelationship = forwardRelationship
                .Any(_ => _.RelatedIncidentId == relatedIncidentId);
            bool backwardHasRelationship = false;

            if (!forwardHasRelationship)
            {
                var backwardRelationship = await _incidentRelationshipRepository
                    .GetByIncidentIdAsync(relatedIncidentId);

                backwardHasRelationship = backwardRelationship
                    .Any(_ => _.RelatedIncidentId == incidentId);
            }

            if (forwardHasRelationship || backwardHasRelationship)
            {
                throw new OcudaException($"Incidents {incidentId} and {relatedIncidentId} are already related.");
            }

            await _incidentRelationshipRepository.AddAsync(new IncidentRelationship
            {
                CreatedAt = _dateTimeProvider.Now,
                CreatedBy = GetCurrentUserId(),
                IncidentId = Math.Min(incidentId, relatedIncidentId),
                RelatedIncidentId = Math.Max(incidentId, relatedIncidentId)
            });

            await _incidentRelationshipRepository.SaveAsync();
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

        public async Task AdjustTypeStatusAsync(int incidentTypeId, bool status)
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

        public async Task<Incident> GetAsync(int incidentId)
        {
            var incident = await _incidentRepository.FindAsync(incidentId);
            if (incident == null)
            {
                return null;
            }

            incident.CreatedByUser = await _userService.GetByIdAsync(incident.CreatedBy);

            var staffs = await _incidentStaffRepository.GetByIncidentIdAsync(incidentId);
            if (staffs?.Count > 0)
            {
                if (incident.Staffs == null)
                {
                    incident.Staffs = new List<IncidentStaff>();
                }
                foreach (var staff in staffs)
                {
                    staff.User = await _userService.GetByIdAsync(staff.UserId);
                    incident.Staffs.Add(staff);
                }
            }

            var participants = await _incidentParticipantRepository
                .GetByIncidentIdAsync(incidentId);
            if (participants?.Count > 0)
            {
                if (incident.Participants == null)
                {
                    incident.Participants = new List<IncidentParticipant>();
                }
                foreach (var participant in participants)
                {
                    incident.Participants.Add(participant);
                }
            }

            var followups = await _incidentFollowupRepository.GetByIncidentIdAsync(incidentId);
            if (followups?.Count > 0)
            {
                incident.Followups = followups;
                foreach (var followup in incident.Followups)
                {
                    followup.CreatedByUser = await _userService.GetByIdAsync(followup.CreatedBy);
                    if (followup.UpdatedBy.HasValue)
                    {
                        followup.UpdatedByUser
                            = await _userService.GetByIdAsync(followup.UpdatedBy.Value);
                    }
                }
            }

            var relateds = await _incidentRelationshipRepository.GetByIncidentIdAsync(incidentId);
            if (relateds?.Count > 0)
            {
                if (incident.RelatedIncidents == null)
                {
                    incident.RelatedIncidents = new List<Incident>();
                }
                foreach (var related in relateds)
                {
                    int relatedIncidentId = related.IncidentId != incidentId
                        ? related.IncidentId
                        : related.RelatedIncidentId;
                    var relatedIncident = await _incidentRepository
                        .GetRelatedAsync(relatedIncidentId);
                    relatedIncident.CreatedByUser = await _userService
                        .GetByIdAsync(relatedIncident.CreatedBy);
                    if (relatedIncident.UpdatedBy.HasValue)
                    {
                        relatedIncident.UpdatedByUser
                            = await _userService.GetByIdAsync(relatedIncident.UpdatedBy.Value);
                    }
                    incident.RelatedIncidents.Add(relatedIncident);
                }
            }

            return incident;
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

                var idsFromFollowups = await _incidentFollowupRepository
                    .IncidentIdsSearchAsync(filter.SearchText);

                var idsFromParticipants = await _incidentParticipantRepository
                    .IncidentIdsSearchAsync(filter.SearchText);

                var userIds = await _userService.FindIdsAsync(new SearchFilter
                {
                    SearchText = filter.SearchText,
                });

                var idsFromStaff = await _incidentStaffRepository.IncidentIdsSearchAsync(userIds);

                filter.IncludeIds = idsFromFollowups
                    .Union(idsFromParticipants)
                    .Union(idsFromStaff)
                    .Distinct();
            }

            return await _incidentRepository.GetPaginatedAsync(filter);
        }

        public async Task<IncidentType> GetTypeAsync(string incidentTypeName)
        {
            return await _incidentTypeRepository.GetAsync(incidentTypeName);
        }

        public async Task UpdateIncidentTypeAsync(int incidentTypeId, string incidentTypeDescription)
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
