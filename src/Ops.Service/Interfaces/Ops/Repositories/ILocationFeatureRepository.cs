using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILocationFeatureRepository : IRepository<LocationFeature, int>
    {
        Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location);
        Task<bool> IsDuplicateAsync(LocationFeature locationfeature);
        Task<List<LocationFeature>> GeAllLocationFeaturesAsync();
        Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId);
    }
}
