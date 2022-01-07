using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class RosterService : BaseService<RosterService>, IRosterService
    {
        public const string AsOfHeading = "As Of Date";
        public const string EmailHeading = "Email Address";
        public const string EmployeeIdHeading = "ID";
        public const string HireDateHeading = "Orig Hire Date";
        public const string NameHeading = "Name";
        public const string PositionHeading = "Position #";
        public const string RehireDateHeading = "Rehire Date";
        public const string ReportsToIdHeading = "Reports to ID";
        public const string ReportsToPosHeading = "Reports to Position";
        public const string TitleHeading = "Working Title";
        public const string UnitHeading = "Unit";

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILdapService _ldapService;
        private readonly ILocationService _locationService;
        private readonly IRosterDetailRepository _rosterDetailRepository;
        private readonly IRosterHeaderRepository _rosterHeaderRepository;
        private readonly IUnitLocationMapRepository _unitLocationMapRepository;
        private readonly IUserRepository _userRepository;

        public RosterService(ILogger<RosterService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDateTimeProvider dateTimeProvider,
            ILdapService ldapService,
            ILocationService locationService,
            IRosterDetailRepository rosterDetailRepository,
            IRosterHeaderRepository rosterHeaderRepository,
            IUnitLocationMapRepository unitLocationMapRepository,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _ldapService = ldapService ?? throw new ArgumentNullException(nameof(ldapService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _rosterDetailRepository = rosterDetailRepository
                ?? throw new ArgumentNullException(nameof(rosterDetailRepository));
            _rosterHeaderRepository = rosterHeaderRepository
                ?? throw new ArgumentNullException(nameof(rosterHeaderRepository));
            _unitLocationMapRepository = unitLocationMapRepository
                ?? throw new ArgumentNullException(nameof(unitLocationMapRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<string> AddUnitMap(int unitId, int locationId)
        {
            var location = await _locationService.GetLocationByIdAsync(locationId);

            if (location == null)
            {
                return $"Unable to find location id {locationId}.";
            }

            var existingMap = await _unitLocationMapRepository.FindAsync(unitId);

            if (existingMap != null)
            {
                var mappedLocation = await _locationService
                    .GetLocationByIdAsync(existingMap.LocationId);

                if (mappedLocation == null)
                {
                    return $"Unit {unitId} is mapped to invalid location {existingMap.LocationId}.";
                }
                else
                {
                    return $"Unit {unitId} is already mapped to location {mappedLocation.Name}.";
                }
            }

            await _unitLocationMapRepository.AddAsync(new UnitLocationMap
            {
                CreatedBy = GetCurrentUserId(),
                CreatedAt = _dateTimeProvider.Now,
                UnitId = unitId,
                LocationId = locationId
            });

            await _unitLocationMapRepository.SaveAsync();

            return null;
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

        public async Task<CollectionWithCount<UnitLocationMap>>
            GetUnitLocationMapsAsync(BaseFilter filter)
        {
            return await _unitLocationMapRepository.GetPaginatedAsync(filter);
        }

        public async Task<RosterUpdate> ImportRosterAsync(int currentUserId, string filename)
        {
            var now = DateTime.Now;
            var rosterHeader = new RosterHeader
            {
                CreatedAt = now,
                CreatedBy = currentUserId
            };

            var rosterDetails = new List<RosterDetail>();
            string filePath = Path.Combine(Path.GetTempPath(), filename);

            int vacancies = 0;

            var unitLocationMap = await _unitLocationMapRepository.GetAllAsync();

            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                int asOfColId = 0;
                int emailColId = 0;
                int employeeIdColId = 0;
                int hireDateColId = 0;
                int nameColId = 0;
                int positionColId = 0;
                int rehireDateColId = 0;
                int reportToIdColId = 0;
                int reportToPosColId = 0;
                int titleColId = 0;
                int unitColId = 0;

                int rows = 0;

                using var excelReader = ExcelReaderFactory.CreateReader(stream);
                while (excelReader.Read())
                {
                    rows++;
                    if (rows == 1)
                    {
                        for (int i = 0; i < excelReader.FieldCount; i++)
                        {
                            switch (excelReader.GetString(i).Trim() ?? $"Column{i}")
                            {
                                case AsOfHeading:
                                    asOfColId = i;
                                    break;

                                case EmailHeading:
                                    emailColId = i;
                                    break;

                                case EmployeeIdHeading:
                                    employeeIdColId = i;
                                    break;

                                case HireDateHeading:
                                    hireDateColId = i;
                                    break;

                                case NameHeading:
                                    nameColId = i;
                                    break;

                                case PositionHeading:
                                    positionColId = i;
                                    break;

                                case RehireDateHeading:
                                    rehireDateColId = i;
                                    break;

                                case ReportsToIdHeading:
                                    reportToIdColId = i;
                                    break;

                                case ReportsToPosHeading:
                                    reportToPosColId = i;
                                    break;

                                case TitleHeading:
                                    titleColId = i;
                                    break;

                                case UnitHeading:
                                    unitColId = i;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        var name = excelReader.GetString(nameColId)?.Trim();

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            try
                            {
                                var entry = new RosterDetail
                                {
                                    RosterHeader = rosterHeader,
                                    Name = name,
                                    PositionNum = int.Parse(excelReader.GetString(positionColId),
                                        CultureInfo.InvariantCulture),
                                    JobTitle = excelReader.GetString(titleColId)?.Trim(),
                                    ReportsToPos
                                        = int.Parse(excelReader.GetString(reportToPosColId),
                                            CultureInfo.InvariantCulture),
                                    AsOf = DateTime.Parse(excelReader.GetString(asOfColId),
                                        CultureInfo.InvariantCulture),
                                    IsVacant = string.Equals(name, "Vacant",
                                        StringComparison.OrdinalIgnoreCase),
                                    Unit = int.Parse(excelReader.GetString(unitColId),
                                        CultureInfo.InvariantCulture),
                                    CreatedAt = now,
                                    CreatedBy = currentUserId
                                };

                                if (!entry.IsVacant)
                                {
                                    entry.ReportsToId
                                        = int.Parse(excelReader.GetString(reportToIdColId),
                                            CultureInfo.InvariantCulture);
                                    entry.EmployeeId
                                        = int.Parse(excelReader.GetString(employeeIdColId),
                                            CultureInfo.InvariantCulture);
                                    entry.EmailAddress = excelReader.GetString(emailColId)?.Trim();

                                    var hireDate = excelReader.GetString(hireDateColId);
                                    if (DateTime.TryParse(hireDate, out DateTime hireDateTime))
                                    {
                                        entry.HireDate = hireDateTime;
                                    }
                                    var rehireDate = excelReader.GetString(rehireDateColId);
                                    if (DateTime.TryParse(rehireDate, out DateTime rehireDateTime))
                                    {
                                        entry.RehireDate = rehireDateTime;
                                    }
                                }
                                else
                                {
                                    vacancies++;
                                }

                                // Check if position has already been added to the list
                                if (!rosterDetails.Select(_ => _.PositionNum)
                                    .Contains(entry.PositionNum))
                                {
                                    rosterDetails.Add(entry);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Roster import error for row {Row}, name {Name}: {ErrorMessage}",
                                    rows,
                                    name,
                                    ex.Message);
                            }
                        }
                    }
                }
            }

            await _rosterHeaderRepository.AddAsync(rosterHeader);
            await _rosterDetailRepository.AddRangeAsync(rosterDetails);

            var users = await _userRepository.GetAllAsync();
            var userAddList = new List<User>();
            var userUpdateList = new List<User>();
            var nonVacantRosterDetails = rosterDetails.Where(_ => !_.IsVacant);

            foreach (var rosterDetail in nonVacantRosterDetails)
            {
                // Check if user exists with the employee id
                var user = users.FirstOrDefault(_ => _.EmployeeId == rosterDetail.EmployeeId);
                if (user == null && !string.IsNullOrWhiteSpace(rosterDetail.EmailAddress))
                {
                    user = users.FirstOrDefault(_ => string.Equals(_.Email,
                        rosterDetail.EmailAddress,
                        StringComparison.OrdinalIgnoreCase));
                    if (user?.ExcludeFromRoster == false)
                    {
                        user.EmployeeId = rosterDetail.EmployeeId;
                    }
                }
                if (user != null)
                {
                    if (!user.ExcludeFromRoster)
                    {
                        user.Unit = rosterDetail.Unit;
                        user.IsInLatestRoster = true;
                        user.LastRosterUpdate = now;
                        user.ServiceStartDate = rosterDetail.RehireDate ?? rosterDetail.HireDate;
                        user.Title = rosterDetail.JobTitle;

                        if (!user.AssociatedLocation.HasValue
                            && unitLocationMap != null
                            && user.Unit.HasValue
                            && unitLocationMap.ContainsKey(user.Unit.Value))
                        {
                            user.AssociatedLocation = unitLocationMap[user.Unit.Value];
                        }

                        userUpdateList.Add(user);
                    }
                }
                else
                {
                    user = new User
                    {
                        CreatedAt = now,
                        CreatedBy = currentUserId,
                        Unit = rosterDetail.Unit,
                        Email = rosterDetail.EmailAddress,
                        EmployeeId = rosterDetail.EmployeeId,
                        IsInLatestRoster = true,
                        LastRosterUpdate = now,
                        Name = rosterDetail.Name,
                        ServiceStartDate = rosterDetail.RehireDate ?? rosterDetail.HireDate,
                        Title = rosterDetail.JobTitle
                    };

                    if (unitLocationMap != null
                        && user.Unit.HasValue
                        && unitLocationMap.ContainsKey(user.Unit.Value))
                    {
                        user.AssociatedLocation = unitLocationMap[user.Unit.Value];
                    }

                    user = _ldapService.LookupByEmail(user);

                    if (string.IsNullOrWhiteSpace(user.Username))
                    {
                        _logger.LogWarning("Unable to find LDAP information for {UserEmail}",
                            user.Email);
                    }

                    userAddList.Add(user);
                }
            }

            var rosterEmployeeIds = nonVacantRosterDetails.Select(_ => _.EmployeeId).ToList();
            var userRemoveList = users
                .Where(_ => _.EmployeeId.HasValue
                    && !_.ExcludeFromRoster
                    && !rosterEmployeeIds.Contains(_.EmployeeId.Value))
                .ToList();

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
                    // Check if the user still exists in ldap
                    userRemoveList[i] = _ldapService.LookupByUsername(userRemoveList[i]);
                    if (userRemoveList[i].LastLdapCheck != userRemoveList[i].LastLdapUpdate)
                    {
                        userRemoveList[i].IsDeleted = true;
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

            // Set user supervisors
            var rosterUsers = userAddList.Union(userUpdateList).ToList();
            foreach (var user in rosterUsers)
            {
                var rosterDetail = rosterDetails.Find(_ => _.EmployeeId == user.EmployeeId);

                var reportsTo
                    = rosterDetails.Find(_ => _.PositionNum == rosterDetail.ReportsToPos);
                while (reportsTo?.IsVacant == true)
                {
                    reportsTo = rosterDetails.Find(_ => _.PositionNum == reportsTo.ReportsToPos);
                }

                if (reportsTo != null)
                {
                    user.SupervisorId = rosterUsers
                        .Where(_ => _.EmployeeId == reportsTo.EmployeeId)
                        .Select(_ => _.Id)
                        .FirstOrDefault();
                }
                else
                {
                    user.SupervisorId = null;
                }

                user.UpdatedAt = DateTime.Now;
                user.UpdatedBy = GetCurrentUserId();
            }

            _userRepository.UpdateRange(rosterUsers);
            await _userRepository.SaveAsync();

            return new RosterUpdate
            {
                Deactivated = userRemoveList,
                New = userAddList,
                TotalRows = rosterDetails.Count,
                VacantCount = vacancies,
                Verified = userUpdateList,
            };
        }

        public async Task<string> RemoveUnitMap(int unitId)
        {
            var existingMap = await _unitLocationMapRepository.FindAsync(unitId);

            if (existingMap == null)
            {
                return $"Unable to find mapping for Unit {unitId}";
            }

            _unitLocationMapRepository.Remove(unitId);
            await _unitLocationMapRepository.SaveAsync();
            return null;
        }
    }
}
