using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Models.Navigation;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavigationService
    {
        Task<Navigation> CreateAsync(Navigation navigation, string siteSetting = null);
        Task DeleteAsync(int id);
        Task<Navigation> EditAsync(Navigation navigation);
        Task<Navigation> GetByIdAsync(int id);
        Task<ICollection<Navigation>> GetNavigationChildrenAsync(int id);
        Task<NavigationRoles> GetNavigationRolesAsync();
        Task<ICollection<Navigation>> GetTopLevelNavigationsAsync();
    }
}
