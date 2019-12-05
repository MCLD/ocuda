using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class SectionService : ISectionService
    {
        private readonly ISectionRepository _sectionRepository;

        public SectionService(ISectionRepository sectionRepository)
        {
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
        }

        public async Task<List<Section>> GetSectionsByNamesAsync(List<string> names)
        {
            if (names.Count > 0)
            {
                var sections = new List<Section>();
                foreach (var name in names.OrderByDescending(_ => _).ToList())
                {
                    sections.Add(await _sectionRepository.GetSectionByNameAsync(name));
                }
                return sections;
            }
            else
            {
                return null;
            }
        }

        public async Task<Section> GetByIdAsync(int id)
        {
            return await _sectionRepository.FindAsync(id);
        }

        public async Task<List<Section>> GetAllSectionsAsync()
        {
            return await _sectionRepository.GetAllSectionsAsync();
        }

        public async Task<Section> GetSectionByStubAsync(string stub)
        {
            return await _sectionRepository.GetSectionByStubAsync(stub);
        }
    }
}
