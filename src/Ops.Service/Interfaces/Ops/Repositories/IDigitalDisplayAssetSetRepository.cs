using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IDigitalDisplayAssetSetRepository : IGenericRepository<DigitalDisplayAssetSet>

    {
        public Task<bool> ExistsAsync(int assetId, int setId);

        public Task<ICollection<DigitalDisplayAssetSet>> GetAssetsBySetsAsync(IEnumerable<int> setIds);

        public Task<ICollection<DigitalDisplayAssetSet>> GetByAssetIdAsync(int assetId);

        public Task<IEnumerable<int>> GetExpiredAsync(DateTime endDate);

        public Task<int> GetSetCountContainingAsset(int assetId);

        public Task<IDictionary<int, int>> GetSetsAssetCountsActiveAsync();

        public Task<IDictionary<int, int>> GetSetsAssetCountsAsync();
    }
}