using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPostRepository : IRepository<Post, int>
    {
        Task<Post> GetByStubAndCategoryIdAsync(string stub, int categoryId);
        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter);
        Task<bool> StubInUseAsync(Post post);
    }
}
