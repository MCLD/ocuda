using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavigationTextRepository : IGenericRepository<NavigationText>
    {
        Task<ICollection<NavigationText>> GetByNavigationIdsAsync(List<int> navigationIds);
    }
}
