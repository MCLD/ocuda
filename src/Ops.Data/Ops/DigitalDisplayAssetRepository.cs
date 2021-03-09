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
    public class DigitalDisplayAssetRepository
        : OpsRepository<OpsContext, DigitalDisplayAsset, int>, IDigitalDisplayAssetRepository
    {
        public DigitalDisplayAssetRepository(Repository<OpsContext> repositoryFacade,
            ILogger<DigitalDisplayAssetRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<DataWithCount<ICollection<DigitalDisplayAsset>>>
            GetPaginatedListAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<DigitalDisplayAsset>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .Include(_ => _.CreatedByUser)
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}