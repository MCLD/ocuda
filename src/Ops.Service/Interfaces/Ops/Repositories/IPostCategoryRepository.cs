using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPostCategoryRepository : IRepository<PostCategory, int>
    {
        Task<IEnumerable<PostCategory>> GetBySectionIdAsync(int sectionId);
        Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
