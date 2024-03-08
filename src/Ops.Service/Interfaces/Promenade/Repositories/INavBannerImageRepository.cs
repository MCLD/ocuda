using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavBannerImageRepository : IGenericRepository<NavBannerImage>
    {
        Task<NavBannerImage> GetByNavBannerIdAsync(int navBannerId, int languageId);
    }
}
