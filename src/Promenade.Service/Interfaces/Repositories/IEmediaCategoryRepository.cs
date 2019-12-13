using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IEmediaCategoryRepository : IGenericRepository<EmediaCategory, int>
    {
        Task<EmediaCategory> GetEmediaCategoriesByIds(int emediaId,int categoryId);
        Task<List<EmediaCategory>> GetEmediaCategoriesByEmediaId(int emediaId);
    }
}

