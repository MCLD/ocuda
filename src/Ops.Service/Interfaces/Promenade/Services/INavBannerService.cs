using Ocuda.Promenade.Models.Entities;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavBannerService
    {
        Task<NavBanner> CreateNoSaveAsync(NavBanner navBanner);

        Task EditAsync(NavBanner navBanner);

        Task DeleteAsync(int navBannerId);

        Task<NavBanner> GetByIdAsync(int navBannerId);

        Task<int?> GetPageLayoutIdForNavBannerAsync(int id);

        Task<string> GetFullImageDirectoryPath(string languageName);

        Task<string> GetUploadImageFilePathAsync(string languageName, string filename);
    }
}
