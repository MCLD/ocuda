using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageRepository : IRepository<Page, int>
    {
        Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BaseFilter filter);
        Task<bool> StubInUseAsync(Page page);
    }
}
