using Ocuda.Promenade.Models.Entities;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavBannerService
    {
        Task<NavBanner> CreateNoSaveAsync(NavBanner navBanner);

        Task EditAsync(NavBanner navBanner);
    }
}
