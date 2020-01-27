using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IEmediaCategoryRepository : IGenericRepository<EmediaCategory>
    {
        Task<EmediaCategory> GetByIdsAsync(int emediaId,int categoryId);
        Task<ICollection<Category>> GetCategoriesByEmediaIdAsync(int emediaId);
    }
}

