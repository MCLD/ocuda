using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFeatureService
    {
        Task<Feature> AddFeatureAsync(Feature feature);

        Task DeleteAsync(int id);

        Task<Feature> EditAsync(Feature feature);

        Task<List<Feature>> GetAllFeaturesAsync();

        Task<Feature> GetFeatureByIdAsync(int featureId);

        Task<Feature> GetFeatureByNameAsync(string featureName);

        Task<Feature> GetFeatureBySegmentIdAsync(int segmentId);

        Task<ICollection<Feature>> GetFeaturesByIdsAsync(IEnumerable<int> featureIds);

        Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(BaseFilter filter);

        Task<DataWithCount<ICollection<Feature>>> PageItemsAsync(FeatureFilter filter);
    }
}