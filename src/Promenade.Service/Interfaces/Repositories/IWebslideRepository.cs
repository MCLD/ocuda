using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IWebslideRepository : IGenericRepository<Webslide>
    {
        Task<Webslide> FindAsync(int id);
    }
}
