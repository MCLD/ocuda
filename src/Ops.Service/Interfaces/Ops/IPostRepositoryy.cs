using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface IPostRepository : IRepository<Post, int>
    {
        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
