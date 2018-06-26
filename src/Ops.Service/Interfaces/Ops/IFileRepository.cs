using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IFileRepository : IRepository<File, int>
    {
        Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
