using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageFeatureRepository : IGenericRepository<PageFeature>
    {
        Task<PageFeature> FindAsync(int id);
        Task<PageFeature> GetIncludingChildrenAsync(int id);
        Task<int?> GetPageHeaderIdForPageFeatureAsync(int id);
        Task<int> GetPageLayoutIdForPageFeatureAsync(int id);
    }
}
