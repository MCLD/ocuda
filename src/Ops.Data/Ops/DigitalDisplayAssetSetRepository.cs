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
    public class DigitalDisplayAssetSetRepository
        : GenericRepository<OpsContext, DigitalDisplayAssetSet>, IDigitalDisplayAssetSetRepository
    {
        public DigitalDisplayAssetSetRepository(Repository<OpsContext> repositoryFacade,
            ILogger<DigitalDisplayAssetSetRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<bool> ExistsAsync(int assetId, int setId)
        {
            return await DbSet
                .AsNoTracking()
                .AnyAsync(_ => _.DigitalDisplayAssetId == assetId
                    && _.DigitalDisplaySetId == setId);
        }

        public async Task<ICollection<DigitalDisplayAssetSet>>
            GetAssetsBySetsAsync(IEnumerable<int> setIds)
        {
            return await DbSet
                .Include(_ => _.DigitalDisplayAsset)
                .Where(_ => setIds.Contains(_.DigitalDisplaySetId))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ICollection<DigitalDisplayAssetSet>> GetByAssetIdAsync(int assetId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DigitalDisplayAssetId == assetId)
                .ToListAsync();
        }

        public async Task<int> GetSetCountContainingAsset(int assetId)
        {
            return await DbSet
                .AsNoTracking()
                .CountAsync(_ => _.DigitalDisplayAssetId == assetId);
        }

        public async Task<IDictionary<int, int>> GetSetsAssetCountsActiveAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsEnabled
                    && _.StartDate <= System.DateTime.Now
                    && _.EndDate >= System.DateTime.Now)
                .GroupBy(_ => _.DigitalDisplaySetId)
                .Select(_ => new { _.Key, Count = _.Count() })
                .ToDictionaryAsync(k => k.Key, v => v.Count);
        }

        public async Task<IDictionary<int, int>> GetSetsAssetCountsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .GroupBy(_ => _.DigitalDisplaySetId)
                .Select(_ => new { _.Key, Count = _.Count() })
                .ToDictionaryAsync(k => k.Key, v => v.Count);
        }
    }
}
