using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category, int>
    {
        Task<List<Category>> GetAllCategories();
    }
}
