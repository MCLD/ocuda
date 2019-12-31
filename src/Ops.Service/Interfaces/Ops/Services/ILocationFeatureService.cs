using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationFeatureService
    {
        Task<List<LocationFeature>> GetAllLocationFeaturesAsync();
        Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(Location location);
        Task<LocationFeature> AddLocationFeatureAsync(LocationFeature locationFeature);
        Task<LocationFeature> EditAsync(LocationFeature locationFeature);
        Task DeleteAsync(int id);
        Task<LocationFeature> GetLocationFeatureByIdAsync(int locationFeatureId);
    }
}
