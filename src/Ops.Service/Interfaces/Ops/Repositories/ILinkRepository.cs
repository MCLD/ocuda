using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILinkRepository : IRepository<Link, int>
    {
        Task<Link> GetByNameAndSectionIdAsync(string name, int sectionId);
        Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
