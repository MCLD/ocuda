using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavigationService
    {
        Task<Navigation> CreateAsync(Navigation navigation);
        Task<ICollection<Navigation>> GetTopLevelNavigationsAsync();
    }
}
