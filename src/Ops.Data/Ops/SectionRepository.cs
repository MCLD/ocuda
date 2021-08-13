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
    }
}