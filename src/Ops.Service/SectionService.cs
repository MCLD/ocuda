using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class SectionService : ISectionService
    {
        private readonly IOcudaCache _cache;
        private readonly ISectionRepository _sectionRepository;

        public SectionService(IOcudaCache cache,
            ISectionRepository sectionRepository)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
        }

        public async Task<ICollection<Section>> GetAllAsync()
        {
            var sections = await _cache
                .GetObjectFromCacheAsync<ICollection<Section>>(Utility.Keys.Cache.OpsSections);

            if (sections == null || sections.Count == 0)
            {
                sections = await _sectionRepository.GetAllAsync();
                await _cache.SaveToCacheAsync(Utility.Keys.Cache.OpsSections, sections, 1);
            }

            return sections;
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