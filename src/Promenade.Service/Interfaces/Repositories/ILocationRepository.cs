using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationRepository : IGenericRepository<Location>
    {
        Task<Location> FindAsync(int id);

        Task<ICollection<int>> GetAllLocationIdsAsync();

        Task<ICollection<Location>> GetAllLocationsAsync();

        Task<int?> GetIdBySlugAsync(string slug);
    }
}