using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Service
{
    public class RosterService
    {
        public const string NameHeading = "Name";
        public const string EmployeeIdHeading = "ID";
        public const string PositionHeading = "Position #";
        public const string TitleHeading = "Working Title";
        public const string ReportsToIdHeading = "Reports to ID";
        public const string ReportsToPosHeading = "Reports to Position";
        public const string EmailHeading = "Email Address";
        public const string AsOfHeading = "As Of Date";

        private readonly ILogger _logger;
        private readonly IRosterDetailRepository _rosterDetailRepository;
        private readonly IRosterHeaderRepository _rosterHeaderRepository;

        public RosterService(ILogger<RosterService> logger,
            IRosterDetailRepository rosterDetailRepository,
            IRosterHeaderRepository rosterHeaderRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rosterDetailRepository = rosterDetailRepository
                ?? throw new ArgumentNullException(nameof(rosterDetailRepository));
            _rosterHeaderRepository = rosterHeaderRepository
                ?? throw new ArgumentNullException(nameof(rosterHeaderRepository));
        }

        public async Task<int> UploadRosterAsync(int currentUserId, string filename)
        {
            var createdAt = DateTime.Now;
            var rosterHeader = new RosterHeader
            {
                CreatedAt = createdAt,
                CreatedBy = currentUserId
            };

            var rosterDetails = new List<RosterDetail>();
            string filePath = Path.Combine(Path.GetTempPath(), filename);
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    int nameColId = 0;
                    int employeeIdColId = 0;
                    int positionColId = 0;
                    int titleColId = 0;
                    int reportToIdColId = 0;
                    int reportToPosColId = 0;
                    int emailColId = 0;
                    int asOfColId = 0;
                    int rows = 0;
                    using (var excelReader = ExcelReaderFactory.CreateBinaryReader(stream))
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
                                var name = excelReader.GetString(nameColId);

                                // Skip over vacant positions
                                if (string.Equals(name, "Vacant",
                                    StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                                var entry = new RosterDetail()
                                {
                                    RosterHeader = rosterHeader,
                                    Name = name,
                                    EmployeeId = int.Parse(excelReader.GetString(employeeIdColId)),
                                    PositionNum = int.Parse(excelReader.GetString(positionColId)),
                                    JobTitle = excelReader.GetString(titleColId),
                                    ReportsToId = int.Parse(excelReader.GetString(reportToIdColId)),
                                    ReportsToPos
                                        = int.Parse(excelReader.GetString(reportToPosColId)),
                                    EmailAddress = excelReader.GetString(emailColId),
                                    AsOf = DateTime.Parse(excelReader.GetString(asOfColId)),
                                    CreatedAt = createdAt,
                                    CreatedBy = currentUserId
                                };

                                // Check if employee has already been added to the list
                                if (rosterDetails.Select(_ => _.EmployeeId)
                                    .Contains(entry.EmployeeId) == false)
                                {
                                    rosterDetails.Add(entry);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                //TODO we didn't create this file here, we shouldn't be responsbile for deleting it here
                //either modify this to create the file here or move deletion to where the file is created
                System.IO.File.Delete(filePath);
            }

            await _rosterHeaderRepository.AddAsync(rosterHeader);
            await _rosterDetailRepository.AddRangeAsync(rosterDetails);
            await _rosterDetailRepository.SaveAsync();

            return rosterDetails.Count();
        }

        public async Task<(RosterHeader RosterDetail,
                           IEnumerable<RosterDetail> NewEmployees,
                           IEnumerable<RosterDetail> RemovedEmployees)> GetRosterChangesAsync()
        {
            var detail = new RosterHeader()
            {
                CreatedAt = DateTime.Now,
                CreatedBy = 1
            };

            var newEmployees = new List<RosterDetail>()
            {
                new RosterDetail
                {
                    Id = 1,
                    Name = "Harry Potter",
                    EmailAddress = "harry@hogwarts.edu",
                    JobTitle = "Student",
                    EmployeeId = 1,
                },
                new RosterDetail
                {
                    Id = 2,
                    Name = "Hermione Granger",
                    EmailAddress = "hermione@hogwarts.edu",
                    JobTitle = "Student",
                    EmployeeId = 2,
                },
                new RosterDetail
                {
                    Id = 3,
                    Name = "Ron Weasley",
                    EmailAddress = "ron@hogwarts.edu",
                    JobTitle = "Student",
                    EmployeeId = 3,
                }
            };

            var removedEmployees = new List<RosterDetail>()
            {
                new RosterDetail
                {
                    Id = 4,
                    Name = "Severus Snape",
                    EmailAddress = "snape@hogwarts.edu",
                    JobTitle = "Potions Master",
                    EmployeeId = 4,
                },
                new RosterDetail
                {
                    Id = 5,
                    Name = "Albus Dumbledore",
                    EmailAddress = "dubledore@hogwarts.edu",
                    JobTitle = "Headmaster",
                    EmployeeId = 5,
                },
                new RosterDetail
                {
                    Id = 6,
                    Name = "Tom Riddle",
                    EmailAddress = "voldemort@hogwarts.edu",
                    JobTitle = "Dark Lord Voldemort",
                    EmployeeId = 6,
                }
            };

            return (detail, newEmployees, removedEmployees);
        }

        public async Task<bool> ApproveRosterChanges(int rosterEntryId)
        {
            var serviceResult = true;

            return serviceResult;
        }
    }
}
