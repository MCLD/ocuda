using Microsoft.AspNetCore.Http;
using Ocuda.Promenade.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface INavBannerService
    {
        Task AddImageNoSaveAsync(NavBannerImage image);

        Task AddLinksAndTextsNoSaveAsync(List<NavBannerLink> links);

        Task AddLinkTextsNoSaveAsync(List<NavBannerLinkText> texts);

        Task<NavBanner> CloneAsync(int navBannerId);

        Task<NavBanner> CreateNoSaveAsync(NavBanner navBanner);

        Task EditAsync(NavBanner navBanner);

        Task DeleteAsync(int navBannerId);

        Task<NavBanner> GetByIdAsync(int navBannerId);

        Task<NavBannerImage> GetImageByNavBannerIdAsync(int navBannerId, int languageId);

        Task<List<NavBannerLink>> GetLinksByNavBannerIdAsync(int navBannerId, int languageId);

        Task<int?> GetPageLayoutIdForNavBannerAsync(int id);

        Task<int?> GetPageHeaderIdAsync(int navBannerId);

        Task<string> GetFullImageDirectoryPath(string languageName);

        string GetImageAssetPath(string fileName, string languageName);

        Task<string> GetUploadImageFilePathAsync(string languageName, string filename);

        void UpdateImageNoSave(NavBannerImage image);

        void UpdateLinksNoSave(List<NavBannerLink> links);

        void UpdateLinkTextNoSave(NavBannerLinkText linkText);

        Task SaveAsync();
    }
}
