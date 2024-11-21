using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IDigitalDisplayService
    {
        public Task<DigitalDisplayAsset> AddAssetAsync(DigitalDisplayAsset asset);

        public Task AddDisplayAsync(DigitalDisplay display);

        public Task AddSetAsync(DigitalDisplaySet displaySet);

        public Task AddUpdateDigitalDisplayAssetSetAsync(DigitalDisplayAssetSet assetSet);

        public Task<IEnumerable<int>> AssignSetAsync(int displayId, int setId);

        public Task<int> ClearExpiredAssetsAsync(DateTime endDate);

        public Task DeleteAssetAsync(int digitalDisplayAssetId);

        public Task DeleteDisplayAsync(int displayId);

        public Task DeleteSetAsync(int setId);

        public Task<DigitalDisplayAsset> FindAssetByChecksumAsync(byte[] checksum);

        public Task<DigitalDisplayAsset> GetAssetAsync(int digitalDisplayAssetId);

        public string GetAssetPath(string fileName);

        public Task<ICollection<DigitalDisplayAssetSet>> GetAssetSetsAsync(int assetId);

        public Task<IDictionary<int, int>> GetAssetSetsCountAsync(IEnumerable<int> assetIds);

        public IDictionary<int, string> GetAssetUrlPaths(IEnumerable<DigitalDisplayAsset> assets);

        public string GetAssetWebPath(DigitalDisplayAsset asset);

        public Task<IEnumerable<DigitalDisplay>> GetByLocationAsync(int locationId);

        public Task<DigitalDisplay> GetDisplayAsync(int displayId);

        public Task<ICollection<DigitalDisplay>> GetDisplaysAsync();

        public Task<ICollection<DigitalDisplayDisplaySet>>
            GetDisplaysSetsAsync(IEnumerable<int> displayIds);

        public Task<(DateTime AsOf, string Message)> GetDisplayStatusAsync(int displayId);

        public Task<IEnumerable<DigitalDisplayCurrentAsset>>
            GetNonExpiredAssetsAsync(int displayId);

        public Task<DataWithCount<ICollection<DigitalDisplayAsset>>>
            GetPaginatedAssetsAsync(BaseFilter filter);

        public Task<DigitalDisplaySet> GetSetAsync(int setId);

        public Task<DigitalDisplaySet> GetSetAsync(string setName);

        public Task<IDictionary<int, int>> GetSetsAssetCountsActiveAsync();

        public Task<IDictionary<int, int>> GetSetsAssetCountsAsync();

        public Task<ICollection<DigitalDisplaySet>> GetSetsAsync();

        public Task<IDictionary<int, int>> GetSetsDisplaysCountsAsync();

        public Task RemoveDigitalDisplayAssetSetAsync(int assetId, int setId);

        public Task SetDisplayStatusAsync(int displayId, string status);

        public Task UpdateDisplayAsync(DigitalDisplay display);

        public Task UpdateSetAsync(DigitalDisplaySet displaySet);
    }
}