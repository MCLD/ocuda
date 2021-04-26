using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IWebslideItemRepository : IGenericRepository<WebslideItem>
    {
        Task<ICollection<WebslideItem>> GetActiveForWebslideAsync(int webslideId);
    }
}
