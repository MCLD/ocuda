using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class UserSyncService : BaseService<UserSyncService>, IUserSyncService
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILdapService _ldapService;
        private readonly ILocationService _locationService;
        private readonly IUserRepository _userRepository;
        private readonly IUserSyncHistoryRepository _userSyncHistoryRepository;
        private readonly IUserSyncLocationRepository _userSyncLocationRepository;

        public UserSyncService(ILogger<UserSyncService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            ILdapService ldapService,
            ILocationService locationService,
            IUserRepository userRepository,
            IUserSyncHistoryRepository userSyncHistoryRepository,
            IUserSyncLocationRepository userSyncLocationRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(ldapService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(userRepository);
            ArgumentNullException.ThrowIfNull(userSyncHistoryRepository);
            ArgumentNullException.ThrowIfNull(userSyncLocationRepository);

            _dateTimeProvider = dateTimeProvider;
            _ldapService = ldapService;
            _locationService = locationService;
            _userRepository = userRepository;
            _userSyncHistoryRepository = userSyncHistoryRepository;
            _userSyncLocationRepository = userSyncLocationRepository;
        }

        public async Task<StatusReport> CheckSyncLocationsAsync()
        {
            var report = new StatusReport
            {
                AsOf = _dateTimeProvider.Now
            };

            var siteLocations = await _locationService.GetAllLocationsAsync();

            var ldapLocations = _ldapService.GetAllLocations();
            var dbLocations = await _userSyncLocationRepository.GetAllAsync();

            foreach (var location in ldapLocations)
            {
                var dbLocation = dbLocations.SingleOrDefault(_ => _.Name == location);
                if (dbLocation == null)
                {
                    report.AddStatus(location, "Not in database", LogLevel.Error);
                }
                else
                {
                    if (dbLocation.MapToLocationId.HasValue)
                    {
                        var site = siteLocations
                            .SingleOrDefault(_ => _.Id == dbLocation.MapToLocationId.Value);
                        if (site == null)
                        {
                            report.AddStatus(location,
                                $"In database, mapped to location {dbLocation.MapToLocationId} which does not exist",
                                LogLevel.Error);
                        }
                        else
                        {
                            report.AddStatus(location,
                                $"In database, mapped to location {dbLocation.Name}",
                                LogLevel.Information);
                        }
                    }
                    else
                    {
                        report.AddStatus(location, "In database, not mapped", LogLevel.Warning);
                    }
                }
            }
            foreach (var dbNotLdapLocation in dbLocations
                .Where(_ => !ldapLocations.Contains(_.Name)))
            {
                report.AddStatus(dbNotLdapLocation.Name,
                    "In database, not in LDAP/Active Directory",
                    LogLevel.Warning);
            }
            return report;
        }

        public async Task<StatusReport> GetImportDetailAsync(int id)
        {
            var detail = await _userSyncHistoryRepository.FindAsync(id)
                ?? throw new OcudaException($"Unable to find import id {id}");

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<StatusReport>(detail.Log);
            }
            catch (JsonException jex)
            {
                throw new OcudaException($"Unable to decode history record: {jex.Message}", jex);
            }
        }

        public async Task<ICollection<UserSyncLocation>> GetLocationsAsync()
        {
            return await _userSyncLocationRepository.GetAllAsync();
        }

        public async Task<CollectionWithCount<UserSyncHistory>> GetPaginatedHeadersAsync(BaseFilter filter)
        {
            return await _userSyncHistoryRepository.GetPaginatedAsync(filter);
        }

        public async Task<StatusReport> SyncDirectoryAsync(bool applyChanges)
        {
            var result = new StatusReport
            {
                AsOf = _dateTimeProvider.Now
            };

            var timer = Stopwatch.StartNew();
            var ldapUsers = _ldapService.GetAllUsers();

            int timerCount = 1;
            long lastStop = timer.ElapsedMilliseconds;
            result.StatusCounts.Add($"Timer {timerCount++}: completed LDAP query (ms)", (int)lastStop);

            var opsUsers = await _userRepository.GetAllAsync();
            result.StatusCounts.Add($"Timer {timerCount++}: Initial all-staff query (ms)",
                (int)(timer.ElapsedMilliseconds - lastStop));
            lastStop = timer.ElapsedMilliseconds;

            var systemUser = await _userRepository.GetSystemAdministratorAsync();

            var locations = await _userSyncLocationRepository.GetAllAsync();
            var locationsToAdd = new List<string>();

            var updatedUsernames = new List<string>();

            var count = 0;

            var newUsers = 0;
            var undeletedUsers = 0;
            var updatedUsers = 0;

            if (applyChanges)
            {
                _logger.LogInformation("Scanning AD for user changes and applying them");
            }
            else
            {
                _logger.LogInformation("Scanning AD for user changes and *not* applying them");
            }

            foreach (var ldapUser in ldapUsers)
            {
                var isNew = false;
                var userChanges = new List<string>();
                var userFieldChanges = new List<string>();

                if (string.IsNullOrEmpty(ldapUser.Username))
                {
                    result.AddStatus(ldapUser.DistinguishedName,
                        "No user found for this DistinguishedName in AD",
                        LogLevel.Warning);
                    _logger.LogWarning("No username found for AD distinguished name {DistinguishedName}",
                        ldapUser.DistinguishedName);
                    continue;
                }

                User opsUser = null;

                var matchingUsers = opsUsers.Where(_ => string.Equals(_.Username,
                    ldapUser.Username,
                    StringComparison.OrdinalIgnoreCase));

                if (matchingUsers?.Count() > 1)
                {
                    _logger.LogError("Taking no action on this record - found multiple users with the same username: {Username}",
                        ldapUser.Username);
                    continue;
                }

                opsUser = matchingUsers.SingleOrDefault();

                if (opsUser == null)
                {
                    try
                    {
                        opsUser = await _userRepository
                            .FindUsernameIncludeDeletedAsync(ldapUser.Username);
                    }
                    catch (InvalidOperationException ioex)
                    {
                        _logger.LogError("Taking no action on this record - {Username}: {ErrorMessage}",
                            ldapUser.Username,
                            ioex.Message);
                        continue;
                    }

                    if (opsUser == null)
                    {
                        // new user
                        opsUser = new User
                        {
                            CreatedAt = result.AsOf,
                            CreatedBy = systemUser.Id,
                            Username = ldapUser.Username
                        };
                        result.AddStatus(opsUser.Username,
                            "Created user from AD record",
                            LogLevel.Information);
                        _logger.LogWarning("Creating user: {Username}", opsUser.Username);
                        newUsers++;
                        isNew = true;
                    }
                    else
                    {
                        //deleted
                        _logger.LogWarning("Undeleting user: {Username}", opsUser.Username);
                        undeletedUsers++;
                        result.AddStatus(opsUser.Username, "Undeleted user", LogLevel.Warning);
                        opsUser.IsDeleted = false;
                    }
                }

                // update user info

                if (ldapUser.Department?.Length > 0 && opsUser.Department != ldapUser.Department)
                {
                    userFieldChanges.Add(nameof(opsUser.Department));
                    opsUser.Department = ldapUser.Department;
                }
                if (ldapUser.Description?.Length > 0
                    && opsUser.Description != ldapUser.Description)
                {
                    userFieldChanges.Add(nameof(opsUser.Description));
                    opsUser.Description = ldapUser.Description;
                }
                if (ldapUser.Name?.Length > 0 && opsUser.Name != ldapUser.Name)
                {
                    userFieldChanges.Add(nameof(opsUser.Name));
                    opsUser.Name = ldapUser.Name;
                }
                if (ldapUser.EmployeeId.HasValue && opsUser.EmployeeId != ldapUser.EmployeeId)
                {
                    userFieldChanges.Add(nameof(opsUser.EmployeeId));
                    opsUser.EmployeeId = ldapUser.EmployeeId;
                }
                if (ldapUser.ServiceStartDate.HasValue
                    && opsUser.ServiceStartDate != ldapUser.ServiceStartDate.Value)
                {
                    userFieldChanges.Add(nameof(opsUser.ServiceStartDate));
                    opsUser.ServiceStartDate = ldapUser.ServiceStartDate;
                }
                if (!string.IsNullOrEmpty(ldapUser.Nickname)
                    && string.IsNullOrEmpty(opsUser.Nickname))
                {
                    userFieldChanges.Add(nameof(opsUser.Nickname));
                    opsUser.Nickname = ldapUser.Nickname;
                }
                if (ldapUser.Email?.Length > 0 && opsUser.Email != ldapUser.Email)
                {
                    userFieldChanges.Add(nameof(opsUser.Email));
                    opsUser.Email = ldapUser.Email;
                }
                if (ldapUser.Mobile?.Length > 0 && opsUser.Mobile != ldapUser.Mobile)
                {
                    userFieldChanges.Add(nameof(opsUser.Mobile));
                    opsUser.Mobile = ldapUser.Mobile;
                }
                if (ldapUser.Phone?.Length > 0 && opsUser.Phone != ldapUser.Phone)
                {
                    userFieldChanges.Add(nameof(opsUser.Phone));
                    opsUser.Phone = ldapUser.Phone;
                }
                if (ldapUser.Title?.Length > 0 && opsUser.Title != ldapUser.Title)
                {
                    userFieldChanges.Add(nameof(opsUser.Title));
                    opsUser.Title = ldapUser.Title;
                }
                if (!opsUser.AssociatedLocation.HasValue
                    && !string.IsNullOrEmpty(ldapUser.PhysicalDeliveryOfficeName))
                {
                    var existingLocation = locations.SingleOrDefault(_ => string.Equals(_.Name,
                        ldapUser.PhysicalDeliveryOfficeName,
                        StringComparison.OrdinalIgnoreCase));
                    if (existingLocation != null)
                    {
                        userFieldChanges.Add(nameof(opsUser.AssociatedLocation));
                        opsUser.AssociatedLocation = existingLocation.MapToLocationId;
                    }
                    else
                    {
                        if (!locationsToAdd.Contains(ldapUser.PhysicalDeliveryOfficeName))
                        {
                            result.AddStatus(opsUser.Username,
                                $"Unknown location: {ldapUser.PhysicalDeliveryOfficeName}",
                                LogLevel.Error);
                            _logger.LogError("New location seen: {LocationName}",
                                ldapUser.PhysicalDeliveryOfficeName);
                            locationsToAdd.Add(ldapUser.PhysicalDeliveryOfficeName);
                        }
                    }
                }

                opsUser.LastLdapCheck = result.AsOf;
                updatedUsernames.Add(opsUser.Username.ToLowerInvariant());
                if (userFieldChanges.Count > 0)
                {
                    result.AddStatus(opsUser.Username,
                        $"Updated fields: {string.Join(", ", userFieldChanges)}");
                    opsUser.LastLdapUpdate = result.AsOf;
                    updatedUsers++;
                    _logger.LogTrace("Updated fields for {Username}: {Fields}",
                    opsUser.Username,
                        string.Join(", ", userFieldChanges));
                }

                if (applyChanges)
                {
                    if (isNew)
                    {
                        await _userRepository.AddAsync(opsUser);
                    }
                    else
                    {
                        _userRepository.Update(opsUser);
                    }
                    if (count % 40 == 0)
                    {
                        _logger.LogDebug("Committing batch of {RecordCount} total records...",
                            count);
                        await _userRepository.SaveAsync();
                    }
                }

                count++;
            }

            if (applyChanges)
            {
                await _userRepository.SaveAsync();
            }

            result.StatusCounts.Add($"Timer {timerCount++}: processed updates (ms)",
                (int)(timer.ElapsedMilliseconds - lastStop));
            lastStop = timer.ElapsedMilliseconds;

            // supervisor update

            opsUsers = await _userRepository.GetAllAsync();

            var staffToSupervisiorMap = new Dictionary<string, string>();

            int updatedSupervisors = 0;

            foreach (var supervisor in ldapUsers.Where(_ => _.DirectReportDNs.Count > 0))
            {
                var supervisorUser = opsUsers.SingleOrDefault(_ => string.Equals(_.Username,
                    supervisor.Username,
                    StringComparison.OrdinalIgnoreCase));

                if (supervisorUser == null)
                {
                    result.AddStatus(supervisor.Username,
                        "Unable to find this supervisor after import",
                        LogLevel.Error);
                    _logger.LogWarning("Unable to find supervisor with username {Username}",
                        supervisor.Username);
                    continue;
                }

                foreach (var directReportDn in supervisor.DirectReportDNs)
                {
                    var staffUsername = ldapUsers
                        .Where(_ => _.DistinguishedName == directReportDn)
                        .Select(_ => _.Username)
                        .SingleOrDefault()
                        ?.Trim();

                    if (staffUsername == null)
                    {
                        result.AddStatus(directReportDn,
                            "Unable to find staff to attach supervisor for this DN",
                            LogLevel.Error);
                        _logger.LogWarning("Unable to determine staff username for DN: {DistinguishedName}",
                            directReportDn);
                        continue;
                    }

                    var staffUser = opsUsers.SingleOrDefault(_ => string.Equals(_.Username,
                        staffUsername,
                        StringComparison.OrdinalIgnoreCase));

                    if (staffUser == null)
                    {
                        result.AddStatus(staffUsername,
                            "Unable to find staff to attach supervisor",
                            LogLevel.Error);
                        _logger.LogWarning("Unable to find staff user username {Username}",
                            staffUsername);
                        continue;
                    }

                    if (staffUser.SupervisorId != supervisorUser.Id)
                    {
                        if (staffUser.SupervisorId.HasValue)
                        {
                            var oldSupervisor
                                = opsUsers.SingleOrDefault(_ => _.Id == staffUser.SupervisorId)
                                ?? await _userRepository
                                    .FindIncludeDeletedAsync(staffUser.SupervisorId.Value);
                            result.AddStatus(staffUsername,
                                $"Update supervisor from {oldSupervisor.Username} to {supervisorUser.Username}");
                            _logger.LogInformation("Updating supervisor for {Staff} from {SupervisorId} to {NewSupervisorId}",
                                staffUsername,
                                oldSupervisor.Username,
                                supervisorUser.Username);
                        }
                        else
                        {
                            result.AddStatus(staffUsername,
                                $"Setting supervisor to {supervisorUser.Username}");
                            _logger.LogInformation("Adding supervisor for {Staff} to {NewSupervisorId}",
                                staffUsername,
                                supervisorUser.Username);
                        }

                        if (!updatedUsernames.Contains(staffUsername))
                        {
                            updatedUsernames.Add(staffUsername);
                        }

                        if (applyChanges)
                        {
                            await _userRepository.UpdateSupervisor(staffUser.Id, supervisorUser.Id);
                        }
                        updatedSupervisors++;
                    }
                }
            }

            result.StatusCounts.Add($"Timer {timerCount++}: processed supervisors (ms)",
                (int)(timer.ElapsedMilliseconds - lastStop));
            lastStop = timer.ElapsedMilliseconds;

            // everyone who hasn't been touched should be deactivated

            var missingUserNames = opsUsers.Where(_ => !string.IsNullOrEmpty(_.Username))
                .Select(_ => _.Username.ToLowerInvariant())
                .Where(_ => !updatedUsernames.Contains(_));

            int deletedUsers = 0;

            foreach (var missingUserName in missingUserNames)
            {
                deletedUsers++;
                result.AddStatus(missingUserName, "Not present in AD, deleted", LogLevel.Warning);
                if (applyChanges)
                {
                    try
                    {
                        await _userRepository.MarkUserDeletedAsync(missingUserName,
                            GetCurrentUserId(),
                            result.AsOf);
                    }
                    catch (OcudaException oex)
                    {
                        result.AddStatus(missingUserName,
                            $"Issue deleting: {oex.Message}",
                            LogLevel.Error);
                    }
                }
            }

            result.StatusCounts.Add($"Timer {timerCount++}: processed deletions (ms)",
                (int)(timer.ElapsedMilliseconds - lastStop));
            lastStop = timer.ElapsedMilliseconds;

            _logger.LogInformation("Total AD {TotalCount} records; {Added} added, {Undeleted} undeleted, {Deleted} deleted, {Updated} updated, {UpdatedSupervisors} updated supervisors",
                count,
                newUsers,
                undeletedUsers,
                deletedUsers,
                updatedUsers,
                updatedSupervisors);

            result.StatusCounts.Add("Added users", newUsers);
            result.StatusCounts.Add("Deleted users", deletedUsers);
            result.StatusCounts.Add("Total records", count);
            result.StatusCounts.Add("Undeleted users", undeletedUsers);
            result.StatusCounts.Add("Updated supervisors", updatedSupervisors);
            result.StatusCounts.Add("Updated users", updatedUsers);
            result.StatusCounts.Add($"Timer {timerCount++}: total elapsed (ms)",
                (int)timer.ElapsedMilliseconds);

            if (applyChanges)
            {
                await _userSyncHistoryRepository.AddAsync(new UserSyncHistory
                {
                    AddedUsers = newUsers,
                    CreatedAt = result.AsOf,
                    CreatedBy = GetCurrentUserId(),
                    DeletedUsers = deletedUsers,
                    Log = System.Text.Json.JsonSerializer.Serialize(result),
                    TotalRecords = count,
                    UndeletedUsers = undeletedUsers,
                    UpdatedUsers = updatedUsers
                });
                await _userSyncHistoryRepository.SaveAsync();
            }

            return result;
        }

        public async Task SyncLocationsAsync()
        {
            var now = _dateTimeProvider.Now;
            var ldapLocations = _ldapService.GetAllLocations();
            var dbLocations = await _userSyncLocationRepository.GetAllAsync();

            foreach (var missingLocation in ldapLocations.Except(dbLocations.Select(_ => _.Name)))
            {
                await _userSyncLocationRepository.AddAsync(new UserSyncLocation
                {
                    CreatedAt = now,
                    CreatedBy = GetCurrentUserId(),
                    Name = missingLocation,
                });
            }
            await _userSyncHistoryRepository.SaveAsync();
        }

        public async Task UpdateLocationMappingAsync(int userSyncLocationId, int? mapToLocationId)
        {
            var locationMapping = await _userSyncLocationRepository.FindAsync(userSyncLocationId);
            locationMapping.MapToLocationId = mapToLocationId;
            locationMapping.UpdatedAt = _dateTimeProvider.Now;
            locationMapping.UpdatedBy = GetCurrentUserId();
            _userSyncLocationRepository.Update(locationMapping);
            await _userSyncLocationRepository.SaveAsync();
        }
    }
}