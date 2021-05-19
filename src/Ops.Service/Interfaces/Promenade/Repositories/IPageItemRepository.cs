using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageItemRepository : IGenericRepository<PageItem>
    {
        Task<PageItem> FindAsync(int id);

        Task<PageItem> GetByLayoutAndOrderAsync(int layoutId, int order);

        Task<int> GetImageFeatureUseCountAsync(int imageFeatureId);

        Task<PageLayout> GetLayoutForItemAsync(int itemId);

        Task<List<PageItem>> GetLayoutSubsequentAsync(int layoutId, int order);

        Task<int?> GetMaxSortOrderForLayoutAsync(int layoutId);
    }
}