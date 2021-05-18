using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IImageFeatureRepository : IGenericRepository<ImageFeature>
    {
        Task<ImageFeature> FindAsync(int id);

        Task<ImageFeature> GetIncludingChildrenAsync(int id);

        Task<int?> GetPageHeaderIdForImageFeatureAsync(int id);

        Task<int> GetPageLayoutIdForImageFeatureAsync(int id);
    }
}