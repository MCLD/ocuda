using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPageRepository : IRepository<Page, int>
    {
        Task<Page> GetByStubAsync(string stub);
        Task<Page> GetByStubAndSectionIdAsync(string stub, int sectionId);
        Task<Page> GetByTitleAndSectionIdAsync(string title, int sectionId);
        Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BlogFilter filter);
        Task<bool> StubInUseAsync(Page page);
    }
}
