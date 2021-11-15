using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<Product> GetActiveBySlugAsync(string slug);
    }
}
