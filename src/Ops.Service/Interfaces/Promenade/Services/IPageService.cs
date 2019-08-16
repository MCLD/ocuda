using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IPageService
    {
        Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BaseFilter filter);
        Task<Page> GetByIdAsync(int id);
        Task<Page> CreateAsync(Page page);
        Task<Page> EditAsync(Page page, bool publish = false);
        Task DeleteAsync(int id);
        Task<bool> StubInUseAsync(Page page);
    }
}
