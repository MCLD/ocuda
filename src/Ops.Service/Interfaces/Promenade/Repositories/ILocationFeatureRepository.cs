using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ILocationFeatureRepository : IGenericRepository<LocationFeature>
    {
        Task<LocationFeature> GetByIdsAsync(int featureId, int locationId);
        Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location);
        Task<bool> IsDuplicateAsync(LocationFeature locationfeature);
        Task<List<LocationFeature>> GeAllLocationFeaturesAsync();
        Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId);
    }
}
