using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationFeatureRepository : IGenericRepository<LocationFeature>
    {
        Task<LocationFeature> GetByIdsAsync(int featureId, int locationId);
        Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId);
    }
}

