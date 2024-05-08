using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface ILocationFeatureService
    {
        Task<LocationFeature> AddLocationFeatureAsync(LocationFeature locationFeature);

        Task DeleteAsync(int featureId, int locationId);

        Task<LocationFeature> EditAsync(LocationFeature locationFeature);

        Task<List<LocationFeature>> GetAllLocationFeaturesAsync();

        Task<LocationFeature> GetByFeatureIdLocationIdAsync(int featureId, int locationId);

        Task<LocationFeature> GetLocationFeatureBySegmentIdAsync(int segmentId);

        Task<List<LocationFeature>> GetLocationFeaturesByLocationAsync(int locationId);
    }
}