using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IFeatureRepository : IGenericRepository<Feature>
    {
        Task<int> CountAsync(FeatureFilter filter);

        Task<Feature> FindAsync(int id);

        Task<List<Feature>> GetAllFeaturesAsync();

        Task<ICollection<Feature>> GetByIdsAsync(IEnumerable<int> featureIds);

        Task<Feature> GetBySegmentIdAsync(int segmentId);

        Task<Feature> GetFeatureByName(string featureName);

        Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(BaseFilter filter);

        Task<bool> IsDuplicateNameAsync(Feature feature);

        Task<bool> IsDuplicateStubAsync(Feature feature);

        Task<ICollection<Feature>> PageAsync(FeatureFilter filter);
    }
}