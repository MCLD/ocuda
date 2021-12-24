using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavigationRepository : IGenericRepository<Navigation>
    {
        Task<Navigation> FindAsync(int id);
        Task<ICollection<Navigation>> GetChildrenAsync(int id);
        Task<ICollection<Navigation>> GetTopLevelNavigationsAsync();
    }
}
