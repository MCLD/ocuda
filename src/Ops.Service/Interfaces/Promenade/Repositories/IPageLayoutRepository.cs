using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageLayoutRepository : IGenericRepository<PageLayout>
    {
        Task<PageLayout> FindAsync(int id);
        Task<ICollection<PageLayout>> GetAllForHeaderIncludingChildrenAsync(int headerId);
        Task<PageLayout> GetIncludingChildrenAsync(int id);
        Task<PageLayout> GetIncludingChildrenWithItemContent(int id);
        Task<DataWithCount<ICollection<PageLayout>>> GetPaginatedListForHeaderAsync(int headerId,
            BaseFilter filter);
    }
}
