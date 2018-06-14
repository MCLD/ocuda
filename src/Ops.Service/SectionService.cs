using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Data.Ops;
using Ocuda.Ops.Models;

namespace Ops.Service
{
    public class SectionService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly SectionRepository _sectionRepository;
        public SectionService(InsertSampleDataService insertSampleDataService,
            SectionRepository sectionRepository)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));

            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
        }

        public async Task<IEnumerable<Section>> GetNavigationAsync()
        {
            var sections = await _sectionRepository
                .ToListAsync(_ => _.SortOrder);
            if (sections == null || sections.Count == 0)
            {
                await _insertSampleDataService.InsertSections();
                sections = await _sectionRepository.ToListAsync(_ => _.SortOrder);
            }

            return sections.Where(_ => !string.IsNullOrEmpty(_.Path));
        }

        public IEnumerable<Calendar> GetCalendars()
        {            
            // TODO repository/database
            // TODO move this somewhere more appropriate
            return new List<Calendar>
            {
                new Calendar
                {
                    IsPinned = true,
                    Name = "Staff Training",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-19 10:00"),
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now,
                },
                new Calendar
                {
                    Name = "Fun Event!",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-08 12:00"),
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now,
                },
                new Calendar
                {
                    Name = "Important Date Reminder",
                    Url = "https://www.google.com/",
                    When = DateTime.Parse("2018-06-12 9:00"),
                    CreatedBy = 1,
                    CreatedAt = DateTime.Now,
                }
            };
        }
    }
}
