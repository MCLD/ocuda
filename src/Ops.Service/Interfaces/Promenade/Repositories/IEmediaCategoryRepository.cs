using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaCategoryRepository : IGenericRepository<EmediaCategory>
    {
        Task<ICollection<EmediaCategory>> GetAllAsync();

        Task<ICollection<EmediaCategory>> GetByEmediaIdAsync(int emediaId);

        Task<ICollection<EmediaCategory>> GetByCategoryIdAsync(int categoryId);

        EmediaCategory GetByEmediaAndCategoryId(int emediaId, int categoryId);
    }
}
