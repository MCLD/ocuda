using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavBannerService
    {
        Task AddImageNoSaveAsync(NavBannerImage image);

        Task AddLinkAsync(NavBannerLink navBannerLink);

        Task AddLinkTextAsync(NavBannerLinkText navBannerLinkText);

        Task<NavBanner> CloneAsync(int navBannerId);

        Task<NavBanner> CreateNoSaveAsync(NavBanner navBanner);

        Task EditAsync(NavBanner navBanner);

        Task<NavBanner> GetByIdAsync(int navBannerId);

        Task<NavBannerImage> GetImageByNavBannerIdAsync(int navBannerId, int languageId);

        Task<ICollection<NavBannerLink>> GetLinksByNavBannerIdAsync(int navBannerId, int languageId);

        Task<int?> GetPageHeaderIdAsync(int navBannerId);

        Task<int?> GetPageLayoutIdForNavBannerAsync(int id);

        Task<string> GetUploadImageFilePathAsync(string languageName);

        Task<int> ImageUseCountAsync(int languageId, string navBannerImageFilename);

        void UpdateImageNoSave(NavBannerImage image);

        Task UpdateLinkAsync(NavBannerLink navBannerLink);

        Task UpdateLinkTextAsync(NavBannerLinkText navBannerLinkText);
    }
}