using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class RosterService : BaseService<RosterService>, IRosterService
    {
        public const string NameHeading = "Name";
        public const string EmployeeIdHeading = "ID";
        public const string PositionHeading = "Position #";
        public const string TitleHeading = "Working Title";
        public const string HireDateHeading = "Orig Hire Date";
        public const string RehireDateHeading = "Rehire Date";
        public const string ReportsToIdHeading = "Reports to ID";
        public const string ReportsToPosHeading = "Reports to Position";
        public const string EmailHeading = "Email Address";
        public const string AsOfHeading = "As Of Date";

        private readonly ILdapService _ldapService;
        private readonly IRosterDetailRepository _rosterDetailRepository;
        private readonly IRosterHeaderRepository _rosterHeaderRepository;
        private readonly IUserRepository _userRepository;

        public RosterService(ILogger<RosterService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILdapService ldapService,
            IRosterDetailRepository rosterDetailRepository,
            IRosterHeaderRepository rosterHeaderRepository,
            IUserRepository userRepository)
            : base(logger, httpContextAccessor)
        {
            _ldapService = ldapService ?? throw new ArgumentNullException(nameof(ldapService));
            _rosterDetailRepository = rosterDetailRepository
                ?? throw new ArgumentNullException(nameof(rosterDetailRepository));
            _rosterHeaderRepository = rosterHeaderRepository
                ?? throw new ArgumentNullException(nameof(rosterHeaderRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<int> ImportRosterAsync(int currentUserId, string filename)
        {
            var now = DateTime.Now;
            var rosterHeader = new RosterHeader
            {
                CreatedAt = now,
                CreatedBy = currentUserId
            };

            var rosterDetails = new List<RosterDetail>();
            string filePath = Path.Combine(Path.GetTempPath(), filename);
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                int nameColId = 0;
                int employeeIdColId = 0;
                int positionColId = 0;
                int titleColId = 0;
                int hireDateColId = 0;
                int rehireDateColId = 0;
                int reportToIdColId = 0;
                int reportToPosColId = 0;
                int emailColId = 0;
                int asOfColId = 0;
                int rows = 0;
                using (var excelReader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (excelReader.Read())
                    {
                        rows++;
                        if (rows == 1)
                        {
                            for (int i = 0; i < excelReader.FieldCount; i++)
                            {
                                switch (excelReader.GetString(i).Trim() ?? $"Column{i}")
                                {
                                    case NameHeading:
                                        nameColId = i;
                                        break;
                                    case EmployeeIdHeading:
                                        employeeIdColId = i;
                                        break;
                                    case PositionHeading:
                                        positionColId = i;
                                        break;
                                    case TitleHeading:
                                        titleColId = i;
                                        break;
                                    case HireDateHeading:
                                        hireDateColId = i;
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
                                    case EmailHeading:
                                        emailColId = i;
                                        break;
                                    case AsOfHeading:
                                        asOfColId = i;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            var name = excelReader.GetString(nameColId)?.Trim();

                            if (!string.IsNullOrWhiteSpace(name))
                            {
                                var entry = new RosterDetail()
                                {
                                    RosterHeader = rosterHeader,
                                    Name = name,
                                    PositionNum = int.Parse(excelReader.GetString(positionColId)),
                                    JobTitle = excelReader.GetString(titleColId)?.Trim(),
                                    ReportsToId = int.Parse(excelReader.GetString(reportToIdColId)),
                                    ReportsToPos = int.Parse(excelReader.GetString(reportToPosColId)),
                                    AsOf = DateTime.Parse(excelReader.GetString(asOfColId)),
                                    IsVacant = string.Equals(name, "Vacant",
                                        StringComparison.OrdinalIgnoreCase),
                                    CreatedAt = now,
                                    CreatedBy = currentUserId
                                };

                                if (!entry.IsVacant)
                                {
                                    entry.EmployeeId = int.Parse(excelReader.GetString(employeeIdColId));
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

                                // Check if position has already been added to the list
                                if (!rosterDetails.Select(_ => _.PositionNum)
                                    .Contains(entry.PositionNum))
                                {
                                    rosterDetails.Add(entry);
                                }
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
                        user.ServiceStartDate = rosterDetail.RehireDate ?? rosterDetail.HireDate;
                        user.Title = rosterDetail.JobTitle;
                        user.LastRosterUpdate = now;
                        user.IsInLatestRoster = true;

                        userUpdateList.Add(user);
                    }
                }
                else
                {
                    user = new User
                    {
                        CreatedAt = now,
                        CreatedBy = currentUserId,
                        Email = rosterDetail.EmailAddress,
                        EmployeeId = rosterDetail.EmployeeId,
                        LastRosterUpdate = now,
                        Name = rosterDetail.Name,
                        ServiceStartDate = rosterDetail.RehireDate ?? rosterDetail.HireDate,
                        Title = rosterDetail.JobTitle,
                        IsInLatestRoster = true
                    };

                    user = _ldapService.LookupByEmail(user);

                    if (string.IsNullOrWhiteSpace(user.Username))
                    {
                        _logger.LogWarning($"Unable to find ldap information for {user.Email}");
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

                var reportsTo = rosterDetails.Find(_ => _.PositionNum == rosterDetail.ReportsToPos);
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

            return rosterDetails.Count;
        }
    }
}
