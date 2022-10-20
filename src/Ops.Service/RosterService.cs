using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Interfaces;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models.Roster;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class RosterService : BaseService<RosterService>, IRosterService
    {
        private const string VacateText = " (Position Vacate:";

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILdapService _ldapService;
        private readonly IRosterDetailRepository _rosterDetailRepository;
        private readonly IRosterDivisionRepository _rosterDivisionRepository;
        private readonly IRosterHeaderRepository _rosterHeaderRepository;
        private readonly IRosterLocationRepository _rosterLocationRepository;
        private readonly IUserRepository _userRepository;

        public RosterService(ILogger<RosterService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            ILdapService ldapService,
            IRosterDetailRepository rosterDetailRepository,
            IRosterHeaderRepository rosterHeaderRepository,
            IRosterDivisionRepository rosterDivisionRepository,
            IRosterLocationRepository rosterLocationRepository,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ldapService = ldapService ?? throw new ArgumentNullException(nameof(ldapService));
            _rosterDetailRepository = rosterDetailRepository
                ?? throw new ArgumentNullException(nameof(rosterDetailRepository));
            _rosterDivisionRepository = rosterDivisionRepository
                ?? throw new ArgumentNullException(nameof(rosterDivisionRepository));
            _rosterHeaderRepository = rosterHeaderRepository
                ?? throw new ArgumentNullException(nameof(rosterHeaderRepository));
            _rosterLocationRepository = rosterLocationRepository
                ?? throw new ArgumentNullException(nameof(rosterLocationRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<RosterComparison> CompareAsync(int rosterHeaderId, bool applyChanges)
        {
            var rosterHeader = await _rosterHeaderRepository.FindAsync(rosterHeaderId);

            if (applyChanges && rosterHeader.IsDisabled)
            {
                throw new OcudaException($"This roster was disabled for import on {rosterHeader.UpdatedAt}");
            }

            if (applyChanges && rosterHeader.IsImported)
            {
                throw new OcudaException($"This roster was imported on {rosterHeader.UpdatedAt}");
            }

            var rosterDetails = await _rosterDetailRepository.GetByHeaderIdAsync(rosterHeaderId);

            if (rosterDetails.Count == 0)
            {
                throw new OcudaException("No details found in roster");
            }

            var databaseLocations = await _rosterLocationRepository.GetAllAsync();

            var rosterLocations = rosterDetails
                .Select(_ => new { _.LocationId, _.LocationName })
                .Distinct()
                .ToDictionary(k => k.LocationId, v => v.LocationName);

            var newLocationIds = rosterLocations.Keys
                .Except(databaseLocations.Select(_ => _.IdInRoster));

            var removedLocationIds = databaseLocations.Select(_ => _.IdInRoster)
                .Except(rosterLocations.Keys);

            foreach (var rosterLocation in rosterLocations)
            {
                var databaseLocation = databaseLocations
                    .SingleOrDefault(_ => _.IdInRoster == rosterLocation.Key);

                if (databaseLocation == null)
                {
                    if (applyChanges && rosterLocation.Key != 0)
                    {
                        await _rosterLocationRepository.AddAsync(new RosterLocation
                        {
                            CreatedAt = rosterHeader.CreatedAt,
                            CreatedBy = GetCurrentUserId(),
                            IdInRoster = rosterLocation.Key,
                            Name = rosterLocation.Value
                        });
                    }
                }
                else
                {
                    if (databaseLocation.Name != rosterLocation.Value?.Trim())
                    {
                        databaseLocation.Name = rosterLocation.Value?.Trim();
                        if (applyChanges)
                        {
                            _rosterLocationRepository.Update(databaseLocation);
                        }
                    }
                }
            }

            if (applyChanges)
            {
                _rosterLocationRepository.RemoveRange(databaseLocations
                    .Where(_ => removedLocationIds.Contains(_.Id))
                    .ToList());

                await _rosterLocationRepository.SaveAsync();
            }

            var validLocations = await _rosterLocationRepository.GetAllAsync();

            var databaseDivisions = await _rosterDivisionRepository.GetAllAsync();

            var rosterDivisions = rosterDetails
                .Select(_ => new { _.DivisionId, _.DivisionName })
                .Distinct()
                .ToDictionary(k => k.DivisionId, v => v.DivisionName);

            var newDivisionIds = rosterDivisions.Keys
                .Except(databaseDivisions.Select(_ => _.IdInRoster));

            var removedDivisionIds = databaseDivisions.Select(_ => _.IdInRoster)
                .Except(rosterDivisions.Keys);

            foreach (var rosterDivision in rosterDivisions)
            {
                var databaseDivision = databaseDivisions
                    .SingleOrDefault(_ => _.IdInRoster == rosterDivision.Key);

                if (databaseDivision == null)
                {
                    if (applyChanges && rosterDivision.Key != 0)
                    {
                        await _rosterDivisionRepository.AddAsync(new RosterDivision
                        {
                            CreatedAt = rosterHeader.CreatedAt,
                            CreatedBy = GetCurrentUserId(),
                            IdInRoster = rosterDivision.Key,
                            Name = rosterDivision.Value
                        });
                    }
                }
                else
                {
                    if (databaseDivision.Name != rosterDivision.Value?.Trim())
                    {
                        databaseDivision.Name = rosterDivision.Value?.Trim();
                        if (applyChanges)
                        {
                            _rosterDivisionRepository.Update(databaseDivision);
                        }
                    }
                }
            }

            if (applyChanges)
            {
                _rosterDivisionRepository.RemoveRange(databaseDivisions
                    .Where(_ => removedDivisionIds.Contains(_.Id))
                    .ToList());

                await _rosterDivisionRepository.SaveAsync();
            }

            var validDivisions = await _rosterDivisionRepository.GetAllAsync();

            var users = await _userRepository.GetAllAsync();
            var userAddList = new List<User>();
            var userUpdateList = new List<User>();
            var userRemoveList = new List<User>();

            var currentUsersNames = users.ToDictionary(k => k.Id, v => v.Name);

            foreach (var rosterDetail in rosterDetails)
            {
                User user = null;
                // look up by employee id, may fail
                if (rosterDetail.EmployeeId.HasValue)
                {
                    user = users.FirstOrDefault(_ => _.EmployeeId == rosterDetail.EmployeeId
                        && !_.IsDeleted);
                }

                if (user != null
                    && string.IsNullOrEmpty(user.Email)
                    && string.IsNullOrEmpty(user.Username))
                {
                    // we have a user by employee id but that record is missing all other info
                    // can we get them by email?
                    var userByEmail = users.FirstOrDefault(_ => _.Email == rosterDetail.EmailAddress
                        && !_.IsDeleted);
                    if (userByEmail != null)
                    {
                        // found by email? let's use that record and delete this record
                        userByEmail.EmployeeId = user.EmployeeId;
                    }
                    if (applyChanges)
                    {
                        _logger.LogDebug("Flagging user {Name} (id {UserId}) for deletion: missing email and username",
                            user.Name,
                            user.Id);
                    }
                    userRemoveList.Add(user);
                    user = userByEmail;
                }

                if (user == null && !string.IsNullOrEmpty(rosterDetail.EmailAddress))
                {
                    // user cannot be found by employee id but does have a roster email address
                    user = users.FirstOrDefault(_ => !_.IsDeleted && string.Equals(_.Email,
                        rosterDetail.EmailAddress,
                        StringComparison.OrdinalIgnoreCase));

                    if (user?.ExcludeFromRoster == false && rosterDetail.EmployeeId.HasValue)
                    {
                        if (applyChanges)
                        {
                            _logger.LogDebug("Found user id {UserId} by email {Email}, fixing their employee id to {EmployeeId}",
                                user.Id,
                                user.Email,
                                rosterDetail.EmployeeId);
                        }
                        // we got this user by email, let's fix their employee id
                        user.EmployeeId = rosterDetail.EmployeeId;
                    }
                }

                string jobTitle = rosterDetail.JobTitle;
                DateTime? setVacateDate = null;

                if (!string.IsNullOrEmpty(jobTitle))
                {
                    int vacateIndex = jobTitle.IndexOf(VacateText);
                    if (vacateIndex > 0)
                    {
                        jobTitle = jobTitle[..vacateIndex];
                        var dateStart = vacateIndex + VacateText.Length;
                        string vacateDateText = rosterDetail.JobTitle[dateStart..].Trim(')');
                        if (DateTime.TryParse(vacateDateText, out var vacateDate))
                        {
                            setVacateDate = vacateDate;
                        }
                        else
                        {
                            if (applyChanges)
                            {
                                _logger.LogWarning("Unable to parse vacate date for roster user {Name}: {VacateDateText} from {FullTitleString}",
                                    rosterDetail.Name,
                                    vacateDateText,
                                    rosterDetail.JobTitle);
                            }
                        }
                    }
                }

                if (user != null)
                {
                    if (!user.ExcludeFromRoster)
                    {
                        if (user.SupervisorId.HasValue
                            && currentUsersNames.ContainsKey(user.SupervisorId.Value))
                        {
                            user.PriorSupervisorName = currentUsersNames[user.SupervisorId.Value]?.Trim();
                        }

                        user.SupervisorName = rosterDetail.ReportsToName?.Trim();

                        user.IsInLatestRoster = true;
                        user.LastRosterUpdate = rosterHeader.CreatedAt;

                        user.PriorName = user.Name;
                        if (user.Name != rosterDetail.Name.Trim())
                        {
                            user.Name = rosterDetail.Name.Trim();
                        }
                        user.PriorVacateDate = user.VacateDate;
                        if (user.VacateDate != setVacateDate)
                        {
                            user.VacateDate = setVacateDate;
                        }
                        user.PriorTitle = user.Title;
                        if (!string.IsNullOrEmpty(jobTitle) && user.Title != jobTitle.Trim())
                        {
                            user.Title = jobTitle.Trim();
                        }

                        user.PriorEmail = user.Email;
                        if (string.IsNullOrEmpty(user.Email)
                            && !string.IsNullOrEmpty(rosterDetail.EmailAddress)
                            && !string.Equals(user.Email,
                                rosterDetail.EmailAddress.Trim(),
                                StringComparison.OrdinalIgnoreCase))
                        {
                            user.Email = rosterDetail.EmailAddress.Trim();
                        }

                        user.PriorAssociatedLocation = user.AssociatedLocation;
                        var location = validLocations
                            .FirstOrDefault(_ => _.IdInRoster == rosterDetail.LocationId
                                && _.MapToLocationId.HasValue);
                        if (location != null
                            && user.AssociatedLocation != location.MapToLocationId.Value)
                        {
                            user.AssociatedLocation = location.MapToLocationId.Value;
                        }

                        if (!user.AssociatedLocation.HasValue)
                        {
                            var division = validDivisions
                                .FirstOrDefault(_ => _.IdInRoster == rosterDetail.DivisionId
                                    && _.MapToLocationId.HasValue);
                            if (division != null
                                && user.AssociatedLocation != division.MapToLocationId.Value)
                            {
                                user.AssociatedLocation = division.MapToLocationId.Value;
                                _logger.LogInformation("For user {Name}, unable to find location id for {LocationName}, using division {DivisionName} ({DivisionId})",
                                    rosterDetail.Name,
                                    rosterDetail.LocationName,
                                    rosterDetail.DivisionName,
                                    rosterDetail.DivisionId);
                            }
                        }

                        if (user.HasUpdates)
                        {
                            user.UpdatedAt = rosterHeader.CreatedAt;
                            user.UpdatedBy = GetCurrentUserId();
                        }

                        userUpdateList.Add(user);
                    }
                }
                else
                {
                    user = new User
                    {
                        CreatedAt = rosterHeader.CreatedAt,
                        CreatedBy = GetCurrentUserId(),
                        Email = rosterDetail.EmailAddress,
                        EmployeeId = rosterDetail.EmployeeId,
                        IsInLatestRoster = true,
                        LastRosterUpdate = rosterHeader.CreatedAt,
                        Name = rosterDetail.Name,
                        SupervisorName = rosterDetail.ReportsToName,
                        Title = jobTitle
                    };

                    if (setVacateDate.HasValue)
                    {
                        user.VacateDate = setVacateDate;
                    }

                    var location = validLocations.FirstOrDefault(_ => _.IdInRoster == rosterDetail.LocationId
                        && _.MapToLocationId.HasValue);
                    if (location != null)
                    {
                        user.AssociatedLocation = location.MapToLocationId.Value;
                    }

                    if (!user.AssociatedLocation.HasValue)
                    {
                        var division = validDivisions.FirstOrDefault(_ => _.IdInRoster == rosterDetail.DivisionId
                            && _.MapToLocationId.HasValue);
                        if (division != null)
                        {
                            user.AssociatedLocation = division.MapToLocationId.Value;
                        }
                    }

                    user = _ldapService.LookupByEmail(user);

                    userAddList.Add(user);
                }
            }

            var rosterEmployeeIds = rosterDetails.Select(_ => _.EmployeeId).ToList();

            var removeList = users.Where(_ => _.EmployeeId.HasValue
                    && !_.ExcludeFromRoster
                    && !rosterEmployeeIds.Contains(_.EmployeeId.Value));
            var inRosterWithoutEmployeeId = new List<User>();

            foreach (var user in removeList)
            {
                if (user.Email != null)
                {
                    var userInRosterByEmail = rosterDetails.SingleOrDefault(_ =>
                        string.Equals(_.EmailAddress,
                            user.Email,
                            StringComparison.OrdinalIgnoreCase));

                    if (userInRosterByEmail != null)
                    {
                        inRosterWithoutEmployeeId.Add(user);
                        if (applyChanges)
                        {
                            _logger.LogInformation("Keeping {Name} ({UserId}) because {Email} is in the roster",
                                user.Name,
                                user.Id,
                                user.Email);
                        }
                    }
                }
                else
                {
                    if (applyChanges)
                    {
                        _logger.LogInformation("Not looking up {Name} ({UserId}) by email because it's blank",
                                            user.Name,
                                            user.Id);
                    }
                }
            }

            userRemoveList.AddRange(removeList.Except(inRosterWithoutEmployeeId));

            if (applyChanges)
            {
                _logger.LogInformation("Applying changes from roster header id {RosterHeaderId}",
                    rosterHeader.Id);

                // process deletions
                for (int i = 0; i < userRemoveList.Count; i++)
                {
                    userRemoveList[i].IsInLatestRoster = false;
                    userRemoveList[i].SupervisorId = null;
                    if (string.IsNullOrWhiteSpace(userRemoveList[i].Username))
                    {
                        userRemoveList[i].IsDeleted = true;
                    }
                    else
                    {
                        //check for user in ldap
                        userRemoveList[i] = _ldapService.LookupByUsername(userRemoveList[i]);
                        if (userRemoveList[i].LastLdapCheck != userRemoveList[i].LastLdapUpdate)
                        {
                            userRemoveList[i].IsDeleted = true;
                        }
                        else
                        {
                            userRemoveList[i].Notes = "Account not removed, still in Active Directory.";
                            _logger.LogInformation("Not deleting user: {User} (id {UserId}) account still present in Active Directory, last seen in roster: {LastSeen}",
                                userRemoveList[i].Id,
                                userRemoveList[i].Username,
                                userRemoveList[i].LastRosterUpdate);
                        }
                    }
                }

                if (userAddList.Count > 0)
                {
                    await _userRepository.AddRangeAsync(userAddList);
                }
                if (userUpdateList.Count > 0)
                {
                    _userRepository.UpdateRange(userUpdateList);
                }
                if (userRemoveList.Count > 0)
                {
                    _userRepository.UpdateRange(userRemoveList);
                }

                await _userRepository.SaveAsync();

                // process supervisors
                var addedUpdatedUsers = userAddList.Union(userUpdateList).ToList();
                foreach (var user in addedUpdatedUsers)
                {
                    if (user == null)
                    {
                        _logger.LogInformation("User is null");
                    }
                    user.SupervisorId = null;

                    var rosterUser = rosterDetails
                        .SingleOrDefault(_ => _.EmployeeId == user.EmployeeId);

                    if (rosterUser == null)
                    {
                        rosterUser = rosterDetails
                            .SingleOrDefault(_ => string.Equals(_.EmailAddress,
                            user.Email,
                            StringComparison.OrdinalIgnoreCase));

                        if (rosterUser == null)
                        {
                            if (applyChanges)
                            {
                                _logger.LogWarning("Could not find roster user with employee id {EmployeeId} or email {Email}",
                                    user.EmployeeId,
                                    user.Email);
                            }
                            continue;
                        }

                        if (applyChanges)
                        {
                            _logger.LogInformation("Could not find roster user with employee id {EmployeeId}, used email {Email}",
                                user.EmployeeId,
                                user.Email);
                        }
                    }

                    var reportsTo = rosterDetails
                        .SingleOrDefault(_ => _.PositionNum == rosterUser.ReportsToPos);

                    if (reportsTo == null)
                    {
                        // try to infer who they report to
                        var nameMatch = rosterDetails
                            .SingleOrDefault(_ => _.Name == rosterUser.ReportsToName);

                        if (nameMatch != null)
                        {
                            _logger.LogInformation("Found supervisor for {User} ({Id}) by matching name {Name}",
                                user.Name,
                                user.Id,
                                rosterUser.ReportsToName);
                            nameMatch.PositionNum = rosterUser.ReportsToPos;
                            reportsTo = nameMatch;
                        }
                    }

                    if (reportsTo != null)
                    {
                        if (reportsTo.EmployeeId.HasValue)
                        {
                            var supervisor = addedUpdatedUsers
                                .SingleOrDefault(_ => _.EmployeeId == reportsTo.EmployeeId);

                            user.SupervisorId = supervisor.Id;
                        }
                        else
                        {
                            _logger.LogWarning("Adding supervisor for {Name} ({Id}) by email: {EmailAddresss}",
                                user.Name,
                                user.Id,
                                reportsTo.EmailAddress);
                            var supervisor = addedUpdatedUsers
                                .SingleOrDefault(_ => string.Equals(_.Email, reportsTo.EmailAddress, StringComparison.OrdinalIgnoreCase));

                            user.SupervisorId = supervisor.Id;
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Cannot determine supervisor of {User} ({Id})",
                            rosterUser.Name,
                            rosterUser.Id);
                    }
                }

                await _userRepository.SaveAsync();

                rosterHeader.UpdatedAt = _dateTimeProvider.Now;
                rosterHeader.UpdatedBy = GetCurrentUserId();
                rosterHeader.IsImported = true;

                _rosterHeaderRepository.Update(rosterHeader);
                await _rosterHeaderRepository.SaveAsync();
            }

            return new RosterComparison
            {
                NewLocations = rosterDetails
                    .Where(_ => newLocationIds.Contains(_.LocationId))
                    .Select(_ => new { _.LocationId, _.LocationName })
                    .Distinct()
                    .ToDictionary(k => k.LocationId, v => v.LocationName),
                NewUsers = userAddList,
                RemovedLocations = databaseLocations
                    .Where(_ => removedLocationIds.Contains(_.Id)),
                RemovedUsers = userRemoveList,
                RosterHeader = rosterHeader,
                UpdatedUsers = userUpdateList,
                TotalRecords = rosterDetails.Count
            };
        }

        public async Task DisableHeaderAsync(int rosterHeaderId)
        {
            var rosterHeader = await _rosterHeaderRepository.FindAsync(rosterHeaderId);

            if (rosterHeader == null)
            {
                throw new OcudaException($"Unable to find roster header id {rosterHeaderId}");
            }

            rosterHeader.IsDisabled = true;
            rosterHeader.UpdatedAt = _dateTimeProvider.Now;
            rosterHeader.UpdatedBy = GetCurrentUserId();

            _rosterHeaderRepository.Update(rosterHeader);
            await _rosterHeaderRepository.SaveAsync();
        }

        public async Task<IEnumerable<IRosterLocationMapping>> GetDivisionsAsync()
        {
            return await _rosterDivisionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<IRosterLocationMapping>> GetLocationsAsync()
        {
            return await _rosterLocationRepository.GetAllAsync();
        }

        public async Task<CollectionWithCount<RosterHeader>>
                                            GetPaginatedRosterHeadersAsync(BaseFilter filter)
        {
            var headers = await _rosterHeaderRepository.GetPaginatedAsync(filter);
            foreach (var item in headers.Data)
            {
                item.DetailCount = await _rosterDetailRepository.GetCountAsync(item.Id);
            }
            return headers;
        }

        public async Task<RosterUpdate> ImportRosterAsync(int currentUserId, string filename)
        {
            string filePath = Path.Combine(Path.GetTempPath(), filename);

            // read file
            var import = ReadFile(filePath);

            var rosterUpdate = new RosterUpdate();

            if (import?.ReportData == null || import.ReportData.Rows.Count == 0)
            {
                rosterUpdate.Issues.Add($"No report data was extracted from the uploaded sheet: {filename}");
                return rosterUpdate;
            }

            var rosterDetails = new List<RosterDetail>();

            int rowCount = 0;

            var rosterHeader = new RosterHeader
            {
                CreatedAt = DateTime.Now,
                CreatedBy = currentUserId
            };

            // insert data from file
            foreach (var dataRowObject in import.ReportData.Rows)
            {
                rowCount++;
                if (dataRowObject is not System.Data.DataRow dataRow)
                {
                    rosterUpdate.Issues.Add($"Row {rowCount}: Invalid data format, unable to extract a row.");
                    continue;
                }

                var rosterDetail = new RosterDetail
                {
                    CreatedAt = rosterHeader.CreatedAt,
                    CreatedBy = currentUserId,
                    EmailAddress = dataRow["Email"]?.ToString(),
                    Name = dataRow["Full Name"]?.ToString(),
                    ReportsToName = dataRow["Worker's Manager - Full Name"]?.ToString(),
                    RosterHeader = rosterHeader
                };

                if (int.TryParse(dataRow["Employee ID"]?.ToString(), out int employeeId) && employeeId > 0)
                {
                    rosterDetail.EmployeeId = employeeId;
                }
                else
                {
                    rosterUpdate.Issues.Add($"Row {rowCount}: Could not parse field employee id {dataRow["Employee ID"]}");
                }

                var position = dataRow["Position"]?.ToString();

                if (!string.IsNullOrEmpty(position)
                    && position.Contains(' ', StringComparison.Ordinal)
                    && position.Length > position.IndexOf(' ', StringComparison.Ordinal))
                {
                    var positionSplit = new[] {
                        position[..position.IndexOf(' ', StringComparison.Ordinal)],
                        position[(position.IndexOf(' ', StringComparison.Ordinal) + 1)..]
                    };

                    if (positionSplit.Length != 2 || positionSplit[0].Length < 3)
                    {
                        rosterUpdate.Issues.Add($"Row {rowCount}: Could not split out required position information from {position}");
                        continue;
                    }
                    if (!int.TryParse(positionSplit[0][1..], out int positionNum))
                    {
                        rosterUpdate.Issues.Add($"Row {rowCount}: Could not determine numeric position number from {positionSplit[0]}");
                    }
                    else
                    {
                        rosterDetail.PositionNum = positionNum;
                    }

                    rosterDetail.JobTitle = positionSplit[1];
                }

                if (!int.TryParse(dataRow["Division ID"].ToString(), out int divisionId))
                {
                    rosterUpdate.Issues.Add($"Row {rowCount}: unable to determine required field division id {dataRow["Division ID"]}");
                    continue;
                }
                else
                {
                    rosterDetail.DivisionId = divisionId;
                    rosterDetail.DivisionName = dataRow["Division Name"]?.ToString();
                    if (string.IsNullOrEmpty(rosterDetail.DivisionName))
                    {
                        rosterUpdate.Issues.Add($"Row {rowCount}: Empty division name.");
                        continue;
                    }
                }

                if (!int.TryParse(dataRow["Location ID"].ToString(), out int locationId))
                {
                    rosterUpdate.Issues.Add($"Row {rowCount}: unable to determine location id {dataRow["Location ID"]}");
                }
                else
                {
                    rosterDetail.LocationId = locationId;
                }

                rosterDetail.LocationName = dataRow["Location Name"]?.ToString();
                if (string.IsNullOrEmpty(rosterDetail.LocationName))
                {
                    rosterUpdate.Issues.Add($"Row {rowCount}: Empty location name.");
                }

                if (int.TryParse(dataRow["Worker's Manager - Employee ID"]?.ToString(), out int managerEmployeeId))
                {
                    rosterDetail.ReportsToId = managerEmployeeId;
                }

                var managerPositionIdString = dataRow["Worker's Manager - Position ID"]?.ToString();

                if (!string.IsNullOrEmpty(managerPositionIdString)
                    && int.TryParse(managerPositionIdString[1..], out int managerPositionId))
                {
                    rosterDetail.ReportsToPos = managerPositionId;
                }

                rosterDetails.Add(rosterDetail);
            }

            await _rosterHeaderRepository.AddAsync(rosterHeader);
            await _rosterDetailRepository.AddRangeAsync(rosterDetails);
            await _rosterDetailRepository.SaveAsync();

            rosterUpdate.RosterHeaderId = rosterHeader.Id;
            rosterUpdate.TotalRows = rosterDetails.Count;

            return rosterUpdate;
        }

        public async Task UpdateDivisionMappingAsync(int divisionId, int? mappingId)
        {
            var divisionMapping = await _rosterDivisionRepository.FindAsync(divisionId);
            divisionMapping.MapToLocationId = mappingId;
            divisionMapping.UpdatedBy = GetCurrentUserId();
            divisionMapping.UpdatedAt = _dateTimeProvider.Now;
            _rosterDivisionRepository.Update(divisionMapping);
            await _rosterDivisionRepository.SaveAsync();
        }

        public async Task UpdateLocationMappingAsync(int locationId, int? mappingId)
        {
            var locationMapping = await _rosterLocationRepository.FindAsync(locationId);
            locationMapping.MapToLocationId = mappingId;
            locationMapping.UpdatedBy = GetCurrentUserId();
            locationMapping.UpdatedAt = _dateTimeProvider.Now;
            _rosterLocationRepository.Update(locationMapping);
            await _rosterLocationRepository.SaveAsync();
        }

        private static ICollection<ImportReportDefinition> GetReportDefinitions()
        {
            var reportDefinitions = new List<ImportReportDefinition>();
            var reportDefinition = new ImportReportDefinition("RPT000d - HCM - Workers by location", 7, 8);
            reportDefinition.Sections.Add(new ImportSectionDefinition("Worker's Manager"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Division ID"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Division Name"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Employee ID"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Employee ID")
            {
                Section = "Worker's Manager"
            });
            reportDefinition.Fields.Add(new ImportFieldDefinition("Full Name"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Full Name")
            {
                Section = "Worker's Manager"
            });
            reportDefinition.Fields.Add(new ImportFieldDefinition("Location ID"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Location Name"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Position"));
            reportDefinition.Fields.Add(new ImportFieldDefinition("Position ID")
            {
                Section = "Worker's Manager"
            });
            reportDefinition.Fields.Add(new ImportFieldDefinition("Email"));

            reportDefinitions.Add(reportDefinition);
            return reportDefinitions;
        }

        private static ImportResult ReadFile(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open);
            using var excelReader = ExcelReaderFactory.CreateReader(stream);

            var rows = 0;
            var issues = new List<string>();

            string? reportName = null;

            ImportReportDefinition? reportDefinition = null;

            var result = new ImportResult();

            while (excelReader.Read())
            {
                rows++;
                if (rows == 1)
                {
                    reportName = excelReader.GetString(0);
                    reportDefinition = GetReportDefinitions()
                        .SingleOrDefault(_ => _.ReportName == reportName);

                    if (reportDefinition == null)
                    {
                        throw new OcudaException($"Cannot find a report import process for a report named {reportName}");
                    }
                }
                else if (reportDefinition != null)
                {
                    if (reportDefinition.SectionLine > 0 && rows < reportDefinition.SectionLine)
                    {
                        var reportParameter = excelReader.GetString(0);
                        if (!string.IsNullOrWhiteSpace(reportParameter))
                        {
                            result.ReportParameters.Add(reportParameter, excelReader.GetString(1));
                        }
                    }
                    if (reportDefinition.SectionLine > 0
                        && reportDefinition.Sections.Count > 0
                        && rows == reportDefinition.SectionLine)
                    {
                        for (int i = 0; i < excelReader.FieldCount; i++)
                        {
                            var section = reportDefinition.Sections
                                .SingleOrDefault(_ => _.SectionName == excelReader.GetString(i)?.Trim());

                            if (section != null)
                            {
                                section.SectionStartingField = i;
                            }
                        }
                    }
                    else if (rows == reportDefinition.HeaderLine)
                    {
                        var missingSections = reportDefinition.Sections
                            .Where(_ => !_.SectionStartingField.HasValue);

                        if (missingSections.Any())
                        {
                            var e = new OcudaException("Unable to find all needed sections");
                            e.Data.Add("MissingSections", missingSections.Select(_ => _.SectionName));
                            throw e;
                        }

                        for (int i = 0; i < excelReader.FieldCount; i++)
                        {
                            string? fieldTitle = excelReader.GetString(i)?.Trim();

                            if (!string.IsNullOrEmpty(fieldTitle))
                            {
                                var section = reportDefinition.Sections
                                    .Where(_ => _.SectionStartingField <= i)
                                    .OrderBy(_ => _.SectionStartingField)
                                    .FirstOrDefault();

                                var fieldDefinition = reportDefinition.Fields
                                    .SingleOrDefault(_ => _.FieldTitle == fieldTitle
                                        && _.Section == section?.SectionName);

                                if (fieldDefinition != null)
                                {
                                    fieldDefinition.FieldPosition = i;

                                    var columnName = section?.SectionName == null
                                        ? fieldDefinition.FieldTitle
                                        : section.SectionName + " - " + fieldDefinition.FieldTitle;
                                    result.ReportData.Columns.Add(columnName, typeof(string));
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rows == reportDefinition.HeaderLine + 1)
                        {
                            // SectionLine + 2 means we should have all our headers
                            var missingFields = reportDefinition.Fields
                                .Where(_ => _.FieldPosition == null);
                            if (missingFields.Any())
                            {
                                var e = new OcudaException("Missing fields, cannot import");
                                e.Data.Add("MissingFields", missingFields);
                                throw e;
                            }
                        }

                        // Process data
                        var row = new Dictionary<string, string>();
                        var rowData = new string[reportDefinition.Fields.Count];
                        int fieldCounter = 0;
                        for (int i = 0; i < excelReader.FieldCount; i++)
                        {
                            var field = reportDefinition.Fields.SingleOrDefault(_ => _.FieldPosition == i);
                            if (field != null)
                            {
                                rowData[fieldCounter++] = excelReader.GetString(i);
                                var key = string.IsNullOrEmpty(field.Section)
                                    ? field.FieldTitle
                                    : field.Section + " - " + field.FieldTitle;

                                row.Add(key, excelReader.GetString(i));
                            }
                        }
                        if (row.Count > 0)
                        {
                            result.ReportData.Rows.Add(rowData);
                        }
                        else
                        {
                            issues.Add($"Row {rows}: no valid data found.");
                        }
                    }
                }
            }

            return result;
        }
    }
}