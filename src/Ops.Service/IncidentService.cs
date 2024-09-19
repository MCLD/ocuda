using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly IEmailService _emailService;
        private readonly IIncidentFollowupRepository _incidentFollowupRepository;
        private readonly IIncidentParticipantRepository _incidentParticipantRepository;
        private readonly IIncidentRelationshipRepository _incidentRelationshipRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IIncidentStaffRepository _incidentStaffRepository;
        private readonly IIncidentTypeRepository _incidentTypeRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IUserService _userService;

        public IncidentService(ILogger<IncidentService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            IEmailService emailService,
            IIncidentFollowupRepository incidentFollowupRepository,
            IIncidentParticipantRepository incidentParticipantRepository,
            IIncidentRelationshipRepository incidentRelationshipRepository,
            IIncidentRepository incidentRepository,
            IIncidentTypeRepository incidentTypeRepository,
            IIncidentStaffRepository incidentStaffRepository,
            ILocationRepository locationRepository,
            ISiteSettingService siteSettingService,
            IUserService userService) : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
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
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<int> AddAsync(Incident incident,
            ICollection<IncidentStaff> staffs,
            ICollection<IncidentParticipant> participants,
            Uri baseUri)
        {
            ArgumentNullException.ThrowIfNull(baseUri);
            ArgumentNullException.ThrowIfNull(incident);
            ArgumentNullException.ThrowIfNull(participants);
            ArgumentNullException.ThrowIfNull(staffs);

            var now = _dateTimeProvider.Now;
            var currentUserId = GetCurrentUserId();

            incident.CreatedAt = now;
            incident.CreatedBy = currentUserId;
            incident.IsVisible = true;

            await _incidentRepository.AddAsync(incident);
            await _incidentRepository.SaveAsync();

            {
                try
                {
                    if (staffs?.Count > 0)
                    {
                        foreach (var staff in staffs)
                        {
                            staff.CreatedAt = now;
                            staff.CreatedBy = currentUserId;
                            staff.IncidentId = incident.Id;
                        }
                        await _incidentStaffRepository.AddRangeAsync(staffs);
                        await _incidentStaffRepository.SaveAsync();
                    }

                    if (participants?.Count > 0)
                    {
                        foreach (var participant in participants)
                        {
                            participant.CreatedAt = now;
                            participant.CreatedBy = currentUserId;
                            participant.IncidentId = incident.Id;
                        }

                        await _incidentParticipantRepository.AddRangeAsync(participants);
                        await _incidentParticipantRepository.SaveAsync();
                    }

                    var link = baseUri.AbsoluteUri.TrimEnd('0') + incident.Id;

                    var emailTemplateId = await _siteSettingService
                        .GetSettingIntAsync(Ops.Models.Keys.SiteSetting.Incident.EmailTemplateId);
                    if (emailTemplateId != default)
                    {
                        var currentUser = await _userService.GetByIdAsync(currentUserId)
                            ?? throw new OcudaException($"Unable to fetch current user with id {currentUserId}");
                        var tags = await GetEmailTagsAsync(incident, currentUser.Name, link);

                        var details = await _emailService.GetDetailsAsync(emailTemplateId,
                            CultureInfo.CurrentUICulture.Name,
                            tags)
                            ?? throw new OcudaException($"Unable to fetch email template id {emailTemplateId} for culture {CultureInfo.CurrentUICulture.Name}");

                        details.ToName = currentUser.Name;
                        details.ToEmailAddress = currentUser.Email;

                        var supervisor = await _userService.GetSupervisorAsync(incident.CreatedBy);
                        if (!string.IsNullOrEmpty(supervisor?.Email))
                        {
                            _logger.LogInformation("Incident email CC: {Name} <{Email}> is the supervisor",
                                supervisor.Name ?? supervisor.Email,
                                supervisor.Email);
                            details.Cc.Add(supervisor.Email, supervisor.Name);
                        }
                        else
                        {
                            _logger.LogInformation("Unable to find supervisor for user {Username} ({UserId})",
                                currentUser.Username,
                                incident.CreatedBy);
                        }

                        var notifyTitleClassIds = await _siteSettingService.GetSettingStringAsync(
                            Ops.Models.Keys.SiteSetting.Incident.NotifyTitleClassificationIds);

                        if (!string.IsNullOrEmpty(notifyTitleClassIds))
                        {
                            var titleClassIds = notifyTitleClassIds.Contains(',',
                                StringComparison.OrdinalIgnoreCase)
                                ? notifyTitleClassIds.Split(',')
                                : new string[] { notifyTitleClassIds };

                            var relatedTitleClassifications = await _userService
                                .GetRelatedTitleClassificationsAsync(currentUserId);

                            foreach (var titleClassId in titleClassIds)
                            {
                                if (int.TryParse(titleClassId, out int numericTitleClassId))
                                {
                                    var titleClass = relatedTitleClassifications
                                        .SingleOrDefault(_ => _.Key.Id == numericTitleClassId);

                                    if (titleClass.Value != null)
                                    {
                                        foreach (var user in titleClass.Value)
                                        {
                                            if (!string.IsNullOrEmpty(user.Email)
                                                && !details.Cc.ContainsKey(user.Email))
                                            {
                                                _logger.LogInformation("Incident email CC: {Name} <{Email}> is classification {TitleClassification}",
                                                    user.Name ?? user.Email,
                                                    user.Email,
                                                    titleClass.Key.Name);
                                                details.Cc.Add(user.Email, user.Name ?? user.Email);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var notifyUserIds = await _siteSettingService.GetSettingStringAsync(
                            Ops.Models.Keys.SiteSetting.Incident.NotifyUserIds);

                        if (!string.IsNullOrEmpty(notifyUserIds))
                        {
                            var userIds = notifyUserIds.Contains(',',
                                StringComparison.OrdinalIgnoreCase)
                                ? notifyUserIds.Split(',')
                                : new string[] { notifyUserIds };

                            foreach (var userId in userIds)
                            {
                                if (int.TryParse(userId, out int numericUserId))
                                {
                                    var user = await _userService.GetByIdAsync(numericUserId);
                                    if (user == null)
                                    {
                                        _logger.LogError("Incidents copied to {UserId} per setting {SiteSetting} but that user can't be found.",
                                            numericUserId,
                                            Ops.Models.Keys.SiteSetting.Incident.NotifyUserIds);
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(user?.Email)
                                            && !details.Cc.ContainsKey(user.Email))
                                        {
                                            _logger.LogInformation("Incident email: {Name} <{Email}> receives all incidents",
                                                user.Name ?? user.Email,
                                                user.Email);
                                            details.Cc.Add(user.Email, user.Name ?? user.Email);
                                        }
                                        else
                                        {
                                            _logger.LogError("Incidents copied to {UserId} per setting {SiteSetting} but that user has no email address.",
                                                numericUserId,
                                                Ops.Models.Keys.SiteSetting.Incident.NotifyUserIds);
                                        }
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("Issue parsing user id {UserId} into a number",
                                        userId);
                                }
                            }
                        }

                        if (incident.LawEnforcementContacted)
                        {
                            var lawEnforcementAddresses
                                = await _siteSettingService.GetSettingStringAsync(
                                    Ops.Models.Keys.SiteSetting.Incident.LawEnforcementAddresses);

                            if (!string.IsNullOrEmpty(lawEnforcementAddresses))
                            {
                                var addresses = lawEnforcementAddresses.Contains(',',
                                    StringComparison.OrdinalIgnoreCase)
                                    ? lawEnforcementAddresses.Split(',')
                                    : new string[] { lawEnforcementAddresses };

                                foreach (var address in addresses)
                                {
                                    if (!details.Cc.ContainsKey(address))
                                    {
                                        _logger.LogInformation("Incident email CC: {Email} receives all law enforcement involvement",
                                            address);
                                        details.Cc.Add(address, address);
                                    }
                                }
                            }
                        }

                        await _emailService.SendAsync(details);
                    }
                }
                catch (Exception ex) when ((ex is OcudaException || ex is NullReferenceException)
                    && ex.InnerException != null)
                {
                    Exception inner = ex;
                    while (inner != null)
                    {
                        inner = inner.InnerException;
                    }
                    _logger.LogInformation(inner,
                        "Issue saving incident report ({OuterMessage}): {ErrorMessage}",
                        ex.Message,
                        inner.Message);
                }
                catch (Exception ex) when (ex is OcudaException || ex is NullReferenceException)
                {
                    _logger.LogInformation(ex,
                        "Issue saving incident report: {ErrorMessage}",
                        ex.Message);
                }
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

            incident.CreatedByUser = await _userService
                .GetByIdIncludeDeletedAsync(incident.CreatedBy);

            var staffs = await _incidentStaffRepository.GetByIncidentIdAsync(incidentId);
            if (staffs?.Count > 0)
            {
                incident.Staffs ??= new List<IncidentStaff>();
                foreach (var staff in staffs)
                {
                    staff.User = await _userService.GetByIdIncludeDeletedAsync(staff.UserId);
                    incident.Staffs.Add(staff);
                }
            }

            var participants = await _incidentParticipantRepository
                .GetByIncidentIdAsync(incidentId);
            if (participants?.Count > 0)
            {
                incident.Participants ??= new List<IncidentParticipant>();
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
                    followup.CreatedByUser = await _userService
                        .GetByIdIncludeDeletedAsync(followup.CreatedBy);
                    if (followup.UpdatedBy.HasValue)
                    {
                        followup.UpdatedByUser
                            = await _userService
                                .GetByIdIncludeDeletedAsync(followup.UpdatedBy.Value);
                    }
                }
            }

            var relateds = await _incidentRelationshipRepository.GetByIncidentIdAsync(incidentId);
            if (relateds?.Count > 0)
            {
                incident.RelatedIncidents ??= new List<Incident>();
                foreach (var related in relateds)
                {
                    int relatedIncidentId = related.IncidentId != incidentId
                        ? related.IncidentId
                        : related.RelatedIncidentId;
                    var relatedIncident = await _incidentRepository
                        .GetRelatedAsync(relatedIncidentId);
                    relatedIncident.CreatedByUser = await _userService
                        .GetByIdIncludeDeletedAsync(relatedIncident.CreatedBy);
                    if (relatedIncident.UpdatedBy.HasValue)
                    {
                        relatedIncident.UpdatedByUser
                            = await _userService
                                .GetByIdIncludeDeletedAsync(relatedIncident.UpdatedBy.Value);
                    }
                    relatedIncident.RelatedByUser = await _userService
                        .GetByIdIncludeDeletedAsync(related.CreatedBy);
                    relatedIncident.RelatedAt = related.CreatedAt;
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
            filter ??= new IncidentFilter();

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

        public async Task<IncidentType> GetTypeAsync(string incidentTypeDescription)
        {
            return await _incidentTypeRepository.GetAsync(incidentTypeDescription);
        }

        public async Task SetVisibilityAsync(int incidentId, bool isVisible)
        {
            await _incidentRepository.SetVisibilityAsync(incidentId, GetCurrentUserId(), isVisible);
        }

        public async Task UpdateIncidentTypeAsync(int incidentTypeId,
            string incidentTypeDescription)
        {
            var type = await _incidentTypeRepository.FindAsync(incidentTypeId)
                ?? throw new OcudaException($"Incident type id {incidentTypeId} not found.");
            type.Description = incidentTypeDescription;
            type.UpdatedAt = _dateTimeProvider.Now;
            type.UpdatedBy = GetCurrentUserId();
            _incidentTypeRepository.Update(type);
            await _incidentTypeRepository.SaveAsync();
        }

        private async Task<IDictionary<string, string>> GetEmailTagsAsync(Incident incident,
            string submitterName,
            string link)
        {
            var incidentTypes = await GetAllIncidentTypesAsync();

            return new Dictionary<string, string>
            {
                { "IncidentId", incident.Id.ToString(CultureInfo.InvariantCulture) },
                { "IncidentType", incidentTypes[incident.IncidentTypeId] },
                { "LawEnforcementContacted", incident.LawEnforcementContacted
                    ? "Law enforcement was contacted."
                    : "Law enforcement was not contacted."},
                { "Link", link },
                { "ReportedBy", !string.IsNullOrEmpty(incident.ReportedByName)
                    ? incident.ReportedByName
                    : submitterName}
            };
        }
    }
}