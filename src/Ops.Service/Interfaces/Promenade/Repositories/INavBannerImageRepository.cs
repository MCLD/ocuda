using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerImageRepository : IGenericRepository<NavBannerImage>
    {
        Task<List<NavBannerImage>> GetAllByNavBannerIdAsync(int navBannerId);
        Task<NavBannerImage> GetByNavBannerIdAsync(int navBannerId, int languageId);
    }
}
