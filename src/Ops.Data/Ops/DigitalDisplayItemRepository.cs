using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class DigitalDisplayItemRepository
        : GenericRepository<OpsContext, DigitalDisplayItem>, IDigitalDisplayItemRepository
    {
        public DigitalDisplayItemRepository(Repository<OpsContext> repositoryFacade,
           ILogger<DigitalDisplayItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<DigitalDisplayItem>> GetByDisplayAsync(int displayId)
        {
            return await DbSet
                .Where(_ => _.DigitalDisplayId == displayId)
                .AsNoTracking()
                .ToListAsync();
        }

        public void RemoveByAssetId(int assetId)
        {
            DbSet.RemoveRange(DbSet.Where(_ => _.DigitalDisplayAssetId == assetId));
        }

        public void RemoveForDisplay(int displayId)
        {
            DbSet.RemoveRange(DbSet.Where(_ => _.DigitalDisplayId == displayId));
        }
    }
}