using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ILinkLibraryRepository : IRepository<LinkLibrary, int>
    {
        Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
