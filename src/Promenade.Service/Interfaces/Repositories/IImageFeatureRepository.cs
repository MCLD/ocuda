using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IImageFeatureRepository : IGenericRepository<ImageFeature>
    {
        Task<ImageFeature> FindAsync(int id);
    }
}