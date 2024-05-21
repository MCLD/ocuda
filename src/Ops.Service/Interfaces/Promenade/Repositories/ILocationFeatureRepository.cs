using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationFeatureRepository : IGenericRepository<LocationFeature>
    {
        Task<List<LocationFeature>> GeAllLocationFeaturesAsync();

        Task<LocationFeature> GetByIdsAsync(int featureId, int locationId);

        Task<LocationFeature> GetBySegmentIdAsync(int segmentId);

        Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location);

        Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId);

        Task<IEnumerable<int>> GetLocationsByFeatureIdAsync(int featureId);

        Task<bool> IsDuplicateAsync(LocationFeature locationfeature);
    }
}