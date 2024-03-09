using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface INavBannerLinkRepository : IGenericRepository<NavBannerLink>
    {
        Task<List<NavBannerLink>> GetByNavBannerIdAsync(int navBannerId);
    }
}
