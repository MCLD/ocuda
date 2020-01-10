using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ILocationRepository : IGenericRepository<Location>
    {
        Task<Location> FindAsync(int id);
        Task<Location> GetLocationByStub(string stub);
        Task<List<Location>> GetAllLocations();
    }
}
