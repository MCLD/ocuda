using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<ICollection<Product>> GetAllNamesSlugsAsync();

        Task<Product> GetAsync(string slug);
    }
}
