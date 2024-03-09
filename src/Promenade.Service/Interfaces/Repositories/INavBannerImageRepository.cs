using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface INavBannerImageRepository : IGenericRepository<NavBannerImage>
    {
        Task<NavBannerImage> GetByNavBannerIdAsync(int navBannerId, int languageId);
    }
}
