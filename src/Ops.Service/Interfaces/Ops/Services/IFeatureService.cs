using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IFeatureService
    {
        Task<Feature> GetFeatureByStubAsync(string featureStub);
        Task<Feature> AddFeatureAsync(Feature feature);
        Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(BaseFilter filter);
        Task<Feature> EditAsync(Feature feature);
        Task DeleteAsync(int id);
    }
}