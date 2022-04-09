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
    public class UnitLocatonMapRepository : OpsRepository<OpsContext, UnitLocationMap, int>,
        IUnitLocationMapRepository
    {
        public UnitLocatonMapRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IUnitLocationMapRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<IDictionary<int, int>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().ToDictionaryAsync(k => k.UnitId, v => v.LocationId);
        }

        public async Task<CollectionWithCount<UnitLocationMap>> GetPaginatedAsync(BaseFilter filter)
        {
            return new CollectionWithCount<UnitLocationMap>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.UnitId)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
