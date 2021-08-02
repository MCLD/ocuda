using System;
using System.Collections.Generic;
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

        public async Task<ICollection<Section>> GetAllAsync()
        {
            return await _sectionRepository.GetAllAsync();
        }

        public async Task<Section> GetByIdAsync(int id)
        {
            return await _sectionRepository.FindAsync(id);
        }

        public async Task<ICollection<Section>> GetByNamesAsync(ICollection<string> names)
        {
            return await _sectionRepository.GetByNames(names);
        }

        public async Task<Section> GetByStubAsync(string stub)
        {
            return await _sectionRepository.GetByStubAsync(stub);
        }

        public async Task<int> GetHomeSectionIdAsync()
        {
            return await _sectionRepository.GetHomeSectionIdAsync();
        }
    }
}