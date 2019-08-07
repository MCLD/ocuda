using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IFeatureRepository : IRepository<Feature, int>
    {
        Task<List<Feature>> GeAllFeaturesAsync();
        Task<Feature> GetFeatureByName(string featureName);
        Task<bool> IsDuplicateAsync(Feature feature);
        Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(BaseFilter filter);
    }
}