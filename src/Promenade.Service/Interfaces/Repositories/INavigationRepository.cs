using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface INavigationRepository : IGenericRepository<Navigation, int>
    {
        Task<ICollection<Navigation>> GetChildren(int parentNavigationId);
    }
}
