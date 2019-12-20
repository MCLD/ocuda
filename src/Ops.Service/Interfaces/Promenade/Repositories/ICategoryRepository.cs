using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<ICollection<Category>> GetAllAsync();

        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter);

        Category GetByClass(string categoryClass);
    }
}
