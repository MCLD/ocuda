using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<Category> GetByNameAsync(string name);
        Task<Category> GetByNameAndSectionIdAsync(string name, int sectionId);
        Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter);
        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(BlogFilter filter);
    }
}
