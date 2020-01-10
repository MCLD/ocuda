using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IFeatureRepository : IGenericRepository<Feature>
    {
        Task<Feature> FindAsync(int id);
        Task<List<Feature>> GetAllFeaturesAsync();
        Task<Feature> GetFeatureByName(string featureName);
        Task<bool> IsDuplicateNameAsync(Feature feature);
        Task<bool> IsDuplicateStubAsync(Feature feature);
        Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(BaseFilter filter);
        Task<ICollection<Feature>> PageAsync(FeatureFilter filter);
        Task<int> CountAsync(FeatureFilter filter);
    }
}