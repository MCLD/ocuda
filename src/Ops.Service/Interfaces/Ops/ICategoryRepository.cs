using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter);
        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
