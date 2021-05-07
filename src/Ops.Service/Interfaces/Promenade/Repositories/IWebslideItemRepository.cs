using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IWebslideItemRepository : IGenericRepository<WebslideItem>
    {
        Task<WebslideItem> FindAsync(int id);

        Task<WebslideItem> GetByWebslideAndOrderAsync(int webslideId, int order);

        Task<ICollection<WebslideItem>> GetByWebslideAsync(int webslideId);

        Task<int?> GetMaxSortOrderForWebslideAsync(int webslideId);

        Task<List<WebslideItem>> GetWebslideSubsequentAsync(int webslideId, int order);
    }
}