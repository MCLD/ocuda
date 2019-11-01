using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;

namespace Ocuda.Ops.Service
{
    public class SectionService : ISectionService
    {
        private readonly ILogger _logger;
        private readonly ISectionRepository _sectionRepository;

        public SectionService(ILogger<SectionService> logger,
            ISectionRepository sectionRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
        }

        public async Task<List<Section>> GetSectionsByNamesAsync(List<string> names)
        {
            if (names.Count>0)
            {
                var sections = new List<Section>();
                foreach (var name in names.OrderByDescending(_ => _).ToList())
                {
                    sections.Add(_sectionRepository.GetSectionByName(name));
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
            return _sectionRepository.GetAllSections();
        }

        public async Task<Section> GetSectionByStubAsync(string stub)
        {
            return _sectionRepository.GetSectionByStub(stub);
        }
    }
}
