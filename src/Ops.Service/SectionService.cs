using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Service
{
    public class SectionService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly ISectionRepository _sectionRepository;
        public SectionService(InsertSampleDataService insertSampleDataService,
            ISectionRepository sectionRepository)
        {
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));

            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
        }

        /// <summary>
        /// Ensure the default section exists.
        /// </summary>
        public async Task EnsureDefaultSectionAsync(int sysadminId)
        {
            var defaultSection = await _sectionRepository.GetDefaultSectionAsync();
            if (defaultSection == null)
            {
                await _sectionRepository.AddAsync(new Ocuda.Ops.Models.Section
                {
                    Name = "Default Section",
                    CreatedAt = DateTime.Now,
                    CreatedBy = sysadminId,
                    SortOrder = 0
                });
                await _sectionRepository.SaveAsync();
            }
        }

        public async Task<IEnumerable<Section>> GetNavigationAsync()
        {
            return await _sectionRepository.GetNavigationSectionsAsync();
        }

        public async Task<Section> GetSectionByPathAsync(string path)
        {
            return await _sectionRepository.GetSectionByPathAsync(path);
        }

        public async Task<IEnumerable<Section>> GetSectionsAsync()
        {
            return await _sectionRepository
                .ToListAsync(_ => _.SortOrder);
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
