using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationFeatureRepository : IGenericRepository<LocationFeature>
    {
        Task<List<LocationFeature>> GetFullLocationFeaturesAsync(string locationStub);
        Task<LocationFeature> GetFullLocationFeatureAsync(string locationStub, string featureStub);
    }
}
