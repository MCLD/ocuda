using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionRepository : OpsRepository<OpsContext, Section, int>, ISectionRepository
    {
        public SectionRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SectionRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<Section>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderByDescending(_ => _.IsHomeSection)
                .ThenBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<ICollection<Section>> GetByNames(ICollection<string> names)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => names.Contains(_.Name))
                .ToListAsync();
        }

        public async Task<Section> GetByStubAsync(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetHomeSectionIdAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsHomeSection == true)
                .Select(_ => _.Id)
                .SingleOrDefaultAsync();
        }
    }
}