using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationFeatureRepository : IGenericRepository<LocationFeature, int>
    {
        Task<List<LocationFeature>> GetLocationFeaturesById(int locationId);
    }
}

