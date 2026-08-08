using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CPI.DirectoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class UserSyncService(ILogger<UserSyncService> logger,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration config,
        IDateTimeProvider dateTimeProvider,
        ILdapService ldapService,
        ILocationService locationService,
        ISiteSettingService siteSettingService,
        IUserManagementService userManagementService,
        IUserRepository userRepository,
        IUserSyncHistoryRepository userSyncHistoryRepository,
        IUserSyncLocationRepository userSyncLocationRepository)
        : BaseService<UserSyncService>(logger, httpContextAccessor),
        IUserSyncService
    {
        private static async Task SendUpdateAsync(
            Job job,
            Func<Job, Task> statusAsync,
            int percentComplete,
            string status)
        {
            if (job != null && statusAsync != null)
            {
                job.PercentComplete = percentComplete;
                job.Status = status;
                await statusAsync(job);
            }
        }

        public async Task<StatusReport> CheckSyncLocationsAsync()
        {
            var report = new StatusReport
            {
                AsOf = dateTimeProvider.Now,
            };

            var siteLocations = await locationService.GetAllLocationsAsync();

            var ldapLocations = ldapService.GetAllLocations();
            var dbLocations = await userSyncLocationRepository.GetAllAsync();

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
            var detail = await userSyncHistoryRepository.FindAsync(id)
                ?? throw new OcudaException($"Unable to find import id {id}");

            try
            {
                return JsonSerializer.Deserialize<StatusReport>(detail.Log);
            }
            catch (JsonException jex)
            {
                throw new OcudaException($"Unable to decode history record: {jex.Message}", jex);
            }
        }

        public async Task<ICollection<UserSyncLocation>> GetLocationsAsync()
        {
            return await userSyncLocationRepository.GetAllAsync();
        }

        public async Task<CollectionWithCount<UserSyncHistory>>
            GetPaginatedHeadersAsync(BaseFilter filter)
        {
            return await userSyncHistoryRepository.GetPaginatedAsync(filter);
        }

        public async Task JobSyncDirectoryAsync(Job job, Func<Job, Task> statusAsync)
        {
            await SyncDirectoryAsync(job.UserId, true, job, statusAsync);
        }

        public async Task<StatusReport> SyncDirectoryAsync(int userId, bool applyChanges)
        {
            return await SyncDirectoryAsync(userId, applyChanges, null, null);
        }

        public async Task SyncLocationsAsync(int userId)
        {
            var now = dateTimeProvider.Now;
            var ldapLocations = ldapService.GetAllLocations();
            var dbLocations = await userSyncLocationRepository.GetAllAsync();

            foreach (var missingLocation in ldapLocations.Except(dbLocations.Select(_ => _.Name)))
            {
                await userSyncLocationRepository.AddAsync(new UserSyncLocation
                {
                    CreatedAt = now,
                    CreatedBy = userId,
                    Name = missingLocation,
                });
            }

            await userSyncHistoryRepository.SaveAsync();
        }

        public async Task UpdateLocationMappingAsync(
            int userId,
            int userSyncLocationId,
            int? mapToLocationId)
        {
            var locationMapping = await userSyncLocationRepository.FindAsync(userSyncLocationId);
            locationMapping.MapToLocationId = mapToLocationId;
            locationMapping.UpdatedAt = dateTimeProvider.Now;
            locationMapping.UpdatedBy = userId;
            userSyncLocationRepository.Update(locationMapping);
            await userSyncLocationRepository.SaveAsync();
        }

        private async Task<StatusReport> SyncDirectoryAsync(
            int userId,
            bool applyChanges,
            Job job,
            Func<Job, Task> statusAsync)
        {
            var result = new StatusReport
            {
                AsOf = dateTimeProvider.Now,
            };

            var timer = Stopwatch.StartNew();
            var ldapUsers = ldapService.GetAllUsers();

            int timerCount = 1;
            long lastStop = timer.ElapsedMilliseconds;
            result.StatusCounts.Add(
                $"Timer {timerCount++}: completed LDAP query (ms)",
                (int)lastStop);

            var opsUsers = await userRepository.GetAllAsync();
            result.StatusCounts.Add(
                $"Timer {timerCount++}: Initial all-staff query (ms)",
                (int)(timer.ElapsedMilliseconds - lastStop));
            lastStop = timer.ElapsedMilliseconds;

            var systemUser = await userRepository.GetSystemAdministratorAsync();

            var locations = await userSyncLocationRepository.GetAllAsync();
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

            DN dnDisabledUsersGroup = null;
            var disabledUsersGroup = config[Configuration.OpsLdapDisabledUsers];
            if (!string.IsNullOrEmpty(disabledUsersGroup))
            {
                dnDisabledUsersGroup = new DN(disabledUsersGroup);
            }

            // we are adding 5% as a guess towards how many not present in AD users there will be
            int total = ldapUsers.Count() + ldapUsers.Count(_ => _.DirectReportDNs.Count > 0) + 5;

            foreach (var ldapUser in ldapUsers)
            {
                var isNew = false;
                var userChanges = new List<string>();
                var userFieldChanges = new List<string>();

                if (string.IsNullOrEmpty(ldapUser.Username))
                {
                    result.AddStatus(
                        ldapUser.DistinguishedName,
                        "No user found for this DistinguishedName in AD",
                        LogLevel.Warning);
                    await SendUpdateAsync(
                        job,
                        statusAsync,
                        count * 100 / total,
                        $"No username found for DN: {ldapUser.DistinguishedName}");
                    _logger.LogWarning(
                        "No username found for AD distinguished name {DistinguishedName}",
                        ldapUser.DistinguishedName);
                    continue;
                }

                User opsUser = null;

                var matchingUsers = opsUsers.Where(_ => string.Equals(_.Username,
                    ldapUser.Username,
                    StringComparison.OrdinalIgnoreCase));

                if (matchingUsers?.Count() > 1)
                {
                    await SendUpdateAsync(
                        job,
                        statusAsync,
                        count * 100 / total,
                        $"Multiple users with this username: {ldapUser.Username}");
                    _logger.LogError(
                        "Taking no action on this record - found multiple users with the same username: {Username}",
                        ldapUser.Username);
                    continue;
                }

                opsUser = matchingUsers.SingleOrDefault();

                if (opsUser == null)
                {
                    try
                    {
                        opsUser = await userRepository
                            .FindUsernameIncludeDeletedAsync(ldapUser.Username);
                    }
                    catch (InvalidOperationException ioex)
                    {
                        _logger.LogError(
                            "Taking no action on this record - {Username}: {ErrorMessage}",
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
                            Username = ldapUser.Username,
                        };
                        result.AddStatus(
                            opsUser.Username,
                            "Created user from AD record",
                            LogLevel.Information);
                        await SendUpdateAsync(
                            job,
                            statusAsync,
                            count * 100 / total,
                            $"Creating user from AD: {opsUser.Username}");
                        _logger.LogWarning("Creating user: {Username}", opsUser.Username);
                        newUsers++;
                        isNew = true;
                    }
                    else
                    {
                        // deleted
                        _logger.LogWarning("Undeleting user: {Username}", opsUser.Username);
                        undeletedUsers++;
                        result.AddStatus(opsUser.Username, "Undeleted user", LogLevel.Warning);
                        await SendUpdateAsync(
                            job,
                            statusAsync,
                            count * 100 / total,
                            $"Undeleting user: {opsUser.Username}");
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

                if (!opsUser.AssociatedLocationManuallySet
                    && !string.IsNullOrEmpty(ldapUser.PhysicalDeliveryOfficeName))
                {
                    var ldapLocation = locations.SingleOrDefault(_ => string.Equals(_.Name,
                        ldapUser.PhysicalDeliveryOfficeName,
                        StringComparison.OrdinalIgnoreCase));

                    if (ldapLocation == null)
                    {
                        if (!locationsToAdd.Contains(ldapUser.PhysicalDeliveryOfficeName))
                        {
                            result.AddStatus(opsUser.Username,
                                $"Unknown location: {ldapUser.PhysicalDeliveryOfficeName}",
                                LogLevel.Error);
                            await SendUpdateAsync(
                                job,
                                statusAsync,
                                count * 100 / total,
                                $"Unknown location: {opsUser.Username} at {ldapUser.PhysicalDeliveryOfficeName}");
                            _logger.LogError("New location seen: {LocationName}",
                                ldapUser.PhysicalDeliveryOfficeName);
                            locationsToAdd.Add(ldapUser.PhysicalDeliveryOfficeName);
                        }
                    }
                    else if (!opsUser.AssociatedLocation.HasValue
                        || ldapLocation.MapToLocationId != opsUser.AssociatedLocation.Value)
                    {
                        userFieldChanges.Add(nameof(opsUser.AssociatedLocation));
                        opsUser.AssociatedLocation = ldapLocation.MapToLocationId;
                    }
                }

                opsUser.LastLdapCheck = result.AsOf;
                updatedUsernames.Add(opsUser.Username.ToLowerInvariant());
                if (userFieldChanges.Count > 0)
                {
                    result.AddStatus(
                        opsUser.Username,
                        $"Updated fields: {string.Join(", ", userFieldChanges)}");
                    await SendUpdateAsync(
                        job,
                        statusAsync,
                        count * 100 / total,
                        $"Updated fields for: {opsUser.Username} - {string.Join(", ", userFieldChanges)}");
                    opsUser.LastLdapUpdate = result.AsOf;
                    updatedUsers++;
                    _logger.LogTrace(
                        "Updated fields for {Username}: {Fields}",
                        opsUser.Username,
                        string.Join(", ", userFieldChanges));
                }

                if (applyChanges)
                {
                    if (isNew)
                    {
                        await userRepository.AddAsync(opsUser);
                    }
                    else
                    {
                        userRepository.Update(opsUser);
                    }

                    if (count % 40 == 0 && count > 0)
                    {
                        _logger.LogDebug("Committing batch of {RecordCount} total records...",
                            count);
                        await SendUpdateAsync(
                            job,
                            statusAsync,
                            count * 100 / total,
                            $"Saving update batch, on record {count} of {total}");
                    }
                }

                count++;
            }

            if (applyChanges)
            {
                await userRepository.SaveAsync();
            }

            result.StatusCounts.Add($"Timer {timerCount++}: processed updates (ms)",
                (int)(timer.ElapsedMilliseconds - lastStop));
            lastStop = timer.ElapsedMilliseconds;

            // supervisor update
            opsUsers = await userRepository.GetAllAsync();

            var staffToSupervisiorMap = new Dictionary<string, string>();

            int updatedSupervisors = 0;

            foreach (var supervisor in ldapUsers.Where(_ => _.DirectReportDNs.Count > 0))
            {
                var supervisorUser = opsUsers.SingleOrDefault(_ => string.Equals(_.Username,
                    supervisor.Username,
                    StringComparison.OrdinalIgnoreCase));

                if (supervisorUser == null)
                {
                    result.AddStatus(
                        supervisor.Username,
                        "Unable to find this supervisor after import",
                        LogLevel.Error);
                    await SendUpdateAsync(
                        job,
                        statusAsync,
                        count * 100 / total,
                        $"Unable to find supervisor after import: {supervisor.Username}");
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
                        if (dnDisabledUsersGroup?.Contains(new DN(directReportDn)) == true)
                        {
                            continue;
                        }

                        result.AddStatus(directReportDn,
                            "Unable to find staff to attach supervisor for this DN, possibly disabled?",
                            LogLevel.Error);
                        await SendUpdateAsync(
                            job,
                            statusAsync,
                            count * 100 / total,
                            $"Unable to find staff to attach supervisor for this DN, disabled?: {directReportDn}");
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
                        await SendUpdateAsync(
                            job,
                            statusAsync,
                            count * 100 / total,
                            $"Unable to find staff to attach supervisor: {staffUsername}");
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
                                ?? await userRepository
                                    .FindIncludeDeletedAsync(staffUser.SupervisorId.Value);
                            result.AddStatus(
                                staffUsername,
                                $"Update supervisor from {oldSupervisor.Username} to {supervisorUser.Username}");
                            await SendUpdateAsync(
                                job,
                                statusAsync,
                                count * 100 / total,
                                $"Update supervisor: {staffUsername} from {oldSupervisor.Username} to {supervisor.Username}");
                            _logger.LogInformation("Updating supervisor for {Staff} from {SupervisorId} to {NewSupervisorId}",
                                staffUsername,
                                oldSupervisor.Username,
                                supervisorUser.Username);
                        }
                        else
                        {
                            result.AddStatus(
                                staffUsername,
                                $"Setting supervisor to {supervisorUser.Username}");
                            await SendUpdateAsync(
                                job,
                                statusAsync,
                                count * 100 / total,
                                $"Adding supervisor: {staffUsername} to {supervisorUser.Username}");
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
                            await userRepository.UpdateSupervisor(staffUser.Id, supervisorUser.Id);
                        }

                        updatedSupervisors++;
                    }
                }

                count++;
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
                await SendUpdateAsync(
                    job,
                    statusAsync,
                    count * 100 / total,
                    $"Not present in AD, deleted: {missingUserName}");
                if (applyChanges)
                {
                    try
                    {
                        await userManagementService.MarkUserDisabledAsync(
                            userId,
                            missingUserName,
                            result.AsOf);
                    }
                    catch (OcudaException oex)
                    {
                        result.AddStatus(missingUserName,
                            $"Issue deleting: {oex.Message}",
                            LogLevel.Error);
                    }
                }

                if (count < total + 1)
                {
                    count++;
                }
            }

            count = total;

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
            await SendUpdateAsync(
                job,
                statusAsync,
                count * 100 / total,
                $"Total AD {count} records; {newUsers} added, {undeletedUsers} undeleted, {deletedUsers} deleted, {updatedUsers} updated, {updatedSupervisors} updated supervisors");

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
                await userSyncHistoryRepository.AddAsync(new UserSyncHistory
                {
                    AddedUsers = newUsers,
                    CreatedAt = result.AsOf,
                    CreatedBy = userId,
                    DeletedUsers = deletedUsers,
                    Log = System.Text.Json.JsonSerializer.Serialize(result),
                    TotalRecords = count,
                    UndeletedUsers = undeletedUsers,
                    UpdatedUsers = updatedUsers,
                });
                await userSyncHistoryRepository.SaveAsync();
            }

            return result;
        }
    }
}
