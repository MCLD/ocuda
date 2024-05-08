using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IFeatureRepository : IGenericRepository<Feature>
    {
        Task<int?> GetIdBySlugAsync(string slug);
    }
}