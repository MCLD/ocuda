using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Data.Ops;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Service
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

        public async Task<IEnumerable<Section>> GetSectionsAsync()
        {
            var sections = await _sectionRepository
                .ToListAsync(_ => _.SortOrder);
            if (sections == null || sections.Count == 0)
            {
                await _insertSampleDataService.InsertSections();
                sections = await _sectionRepository.ToListAsync(_ => _.SortOrder);
            }

            return sections;
        }

        public async Task<int> GetSectionCountAsync()
        {
            return await _sectionRepository.CountAsync();
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

        public async Task<Section> GetSectionByIdAsync(int id)
        {
            return await _sectionRepository.FindAsync(id);
        }

        public async Task<Section> CreateSectionAsync(Section section)
        {
            section.CreatedAt = DateTime.Now;
            await _sectionRepository.AddAsync(section);
            await _sectionRepository.SaveAsync();

            return section;
        }

        public async Task<Section> EditSectionAsync(Section section)
        {
            //TODO fix edit logic
            var currentSection = await _sectionRepository.FindAsync(section.Id);
            currentSection.Name = section.Name;
            _sectionRepository.Update(currentSection);
            return section;
        }

        public async Task DeleteSectionAsync(int id)
        {
            _sectionRepository.Remove(id);
            await _sectionRepository.SaveAsync();
        }
    }
}
