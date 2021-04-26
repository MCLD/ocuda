using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IWebslideRepository : IGenericRepository<Webslide>
    {
        Task<Webslide> FindAsync(int id);
        Task<Webslide> GetIncludingChildrenAsync(int id);
        Task<int?> GetPageHeaderIdForWebslideAsync(int id);
        Task<int> GetPageLayoutIdForWebslideAsync(int id);
    }
}
