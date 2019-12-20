using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaCategoryRepository : IRepository<EmediaCategory, int>
    {
        Task<ICollection<EmediaCategory>> GetAllAsync();

        Task<ICollection<EmediaCategory>> GetByEmediaIdAsync(int emediaId);

        Task<ICollection<EmediaCategory>> GetByCategoryIdAsync(int categoryId);

        EmediaCategory GetByEmediaAndCategoryId(int emediaId, int categoryId);
    }
}
