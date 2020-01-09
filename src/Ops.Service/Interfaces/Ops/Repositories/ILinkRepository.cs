using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILinkRepository : IOpsRepository<Link, int>
    {
        Task<Link> GetLatestByLibraryIdAsync(int id);
        Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter);
        Task<List<Link>> GetFileLibraryFilesAsync(int id);
    }
}
