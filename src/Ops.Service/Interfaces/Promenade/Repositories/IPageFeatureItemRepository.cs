using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageFeatureItemRepository : IGenericRepository<PageFeatureItem>
    {
        Task<PageFeatureItem> FindAsync(int id);

        Task<PageFeatureItem> GetByFeatureAndOrderAsync(int featureId, int order);

        Task<ICollection<PageFeatureItem>> GetByPageFeatureAsync(int featureId);

        Task<int?> GetMaxSortOrderForPageFeatureAsync(int featureId);

        Task<List<PageFeatureItem>> GetPageFeatureSubsequentAsync(int pageFeatureId, int order);
    }
}