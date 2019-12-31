using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFeatureService
    {
        Task<Feature> GetFeatureByNameAsync(string featureName);
        Task<Feature> AddFeatureAsync(Feature feature);
        Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(BaseFilter filter);
        Task<Feature> EditAsync(Feature feature);
        Task DeleteAsync(int id);
        Task<Feature> GetFeatureByIdAsync(int featureId);
        Task<DataWithCount<ICollection<Feature>>> PageItemsAsync(FeatureFilter filter);
        Task<List<Feature>> GetAllFeaturesAsync();
    }
}