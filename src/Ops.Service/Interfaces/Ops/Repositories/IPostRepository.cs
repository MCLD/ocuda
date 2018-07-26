using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPostRepository : IRepository<Post, int>
    {
        Task<Post> GetByStubAsync(string stub);
        Task<Post> GetByStubAndSectionIdAsync(string stub, int sectionId);
        Task<Post> GetByTitleAndSectionIdAsync(string title, int sectionId);
        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter);
        Task<bool> StubInUseAsync(Post post);
    }
}
