using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageFeatureRepository : IGenericRepository<PageFeature>
    {
        Task<PageFeature> FindAsync(int id);
    }
}
