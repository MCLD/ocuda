using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class IncidentTypeRepository
        : OpsRepository<OpsContext, IncidentType, int>, IIncidentTypeRepository
    {
        public IncidentTypeRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IncidentTypeRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<IncidentType>> GetActiveAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .OrderBy(_ => _.Description)
                .ToListAsync();
        }

        public async Task<ICollection<IncidentType>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().ToListAsync();
        }

        public async Task<CollectionWithCount<IncidentType>> GetAsync(BaseFilter filter)
        {
            return new CollectionWithCount<IncidentType>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Description)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<IncidentType> GetAsync(string incidentTypeDescription)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Description == incidentTypeDescription);
        }
    }
}