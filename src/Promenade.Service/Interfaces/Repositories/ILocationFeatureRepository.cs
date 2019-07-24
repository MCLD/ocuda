using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationFeatureRepository : IGenericRepository<LocationFeature, int>
    {
        Task<LocationFeature> GetLocationFeaturesByIds(int locationId,int featureId);
        Task<List<LocationFeature>> GetLocationFeaturesByLocationId(int locationId);
    }
}

