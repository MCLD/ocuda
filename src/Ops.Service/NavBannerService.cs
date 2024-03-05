using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using System;
using Ocuda.Utility.Exceptions;
using System.IO;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using System.Globalization;
using System.Collections.Generic;

namespace Ocuda.Ops.Service
{
    public class NavBannerService : BaseService<NavBannerService>, INavBannerService
    {
        private readonly INavBannerRepository _navBannerRepository;
        private readonly INavBannerImageRepository _navBannerImageRepository;
        private readonly ISiteSettingService _siteSettingService;
        private readonly INavBannerLinkRepository _navBannerLinkRepository;
        private readonly INavBannerLinkTextRepository _navBannerLinkTextRepository;
        private const string AssetBasePath = "assets";
        private const string ImagesFilePath = "images";
        private const string NavBannerFilePath = "navbanner";

        public NavBannerService(ILogger<NavBannerService> logger,
            IHttpContextAccessor httpContextAccessor,
            INavBannerRepository navBannerRepository,
            INavBannerImageRepository navBannerImageRepository,
            INavBannerLinkRepository navBannerLinkRepository,
            ISiteSettingService siteSettingService,
            INavBannerLinkTextRepository navBannerLinkTextRepository) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(navBannerRepository);
            ArgumentNullException.ThrowIfNull(navBannerImageRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkRepository);
            ArgumentNullException.ThrowIfNull(siteSettingService);
            ArgumentNullException.ThrowIfNull(navBannerLinkTextRepository);

            _navBannerRepository = navBannerRepository;
            _navBannerImageRepository = navBannerImageRepository;
            _navBannerLinkRepository = navBannerLinkRepository;
            _siteSettingService = siteSettingService;
            _navBannerLinkTextRepository = navBannerLinkTextRepository;
        }

        public async Task AddImageAsync(NavBannerImage image)
        {
            ArgumentNullException.ThrowIfNull(image);

            image.ImageAltText = image.ImageAltText.Trim();

            await _navBannerImageRepository.AddAsync(image);
            await _navBannerRepository.SaveAsync();
        }

        public async Task AddLinksAsync(List<NavBannerLink> links)
        {
            ArgumentNullException.ThrowIfNull(links);

            foreach (var link in links)
            {
                await _navBannerLinkRepository.AddAsync(link);
                await _navBannerLinkTextRepository.AddAsync(link.Text);
            }

            await _navBannerRepository.SaveAsync();
        }

        public async Task<NavBanner> CreateNoSaveAsync(NavBanner navBanner)
        {
            ArgumentNullException.ThrowIfNull(navBanner);

            navBanner.Name = navBanner.Name?.Trim();

            await _navBannerRepository.AddAsync(navBanner);
            return navBanner;
        }

        public async Task EditAsync(NavBanner navBanner)
        {
            ArgumentNullException.ThrowIfNull(navBanner);

            var updateNavBanner = await _navBannerRepository.GetByIdAsync(navBanner.Id);
            if (navBanner != null)
            {
                updateNavBanner.Name = navBanner.Name;
            }
            _navBannerRepository.Update(updateNavBanner);
            await _navBannerRepository.SaveAsync();
        }

        public async Task<List<NavBannerLink>> GetLinksByNavBannerIdAsync(int navBannerId, int languageId)
        {
            var links = await _navBannerLinkRepository.GetLinksByNavBannerIdAsync(navBannerId);

            if (links?.Count > 0)
            {
                foreach(var link in links)
                {
                    link.Text = await _navBannerLinkTextRepository.GetLinkTextAsync(link.Id, languageId);
                }
            }

            return links;
        }

        public async Task DeleteAsync(int navBannerId)
        {
            var navBanner = await _navBannerRepository.GetByIdAsync(navBannerId);
            _navBannerRepository.Remove(navBanner);
            await _navBannerRepository.SaveAsync();
        }

        public async Task<NavBanner> GetByIdAsync(int navBannerId)
        {
            return await _navBannerRepository.GetByIdAsync(navBannerId)
                ?? throw new OcudaException("NavBanner does not exist.");
        }

        public async Task<NavBannerImage> GetImageByNavBannerIdAsync(int navBannerId)
        {
            return await _navBannerImageRepository.GetByNavBannerIdAsync(navBannerId);
        }

        public async Task<int?> GetPageLayoutIdForNavBannerAsync(int id)
        {
            return await _navBannerRepository.GetPageLayoutIdForNavBannerAsync(id);
        }

        public async Task<string> GetFullImageDirectoryPath(string languageName)
        {
            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            var filePath = Path.Combine(basePath,
                ImagesFilePath,
                languageName,
                NavBannerFilePath);

            if (!Directory.Exists(filePath))
            {
                _logger.LogInformation("Creating nav banner image directory: {Path}",
                    filePath);
                Directory.CreateDirectory(filePath);
            }

            return filePath;
        }

        public string GetImageAssetPath(string fileName, string languageName)
        {
            return Path.Combine(
                AssetBasePath,
                ImagesFilePath, 
                languageName,
                fileName);
        }

        public async Task<string> GetUploadImageFilePathAsync(string languageName, string filename)
        {
            var imagePath = await GetFullImageDirectoryPath(languageName);
            var fullFilePath = Path.Combine(imagePath, filename);

            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }

            return fullFilePath;
        }
    }
}
