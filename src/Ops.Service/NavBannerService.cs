using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class NavBannerService : BaseService<NavBannerService>, INavBannerService
    {
        private const string NavBannerFilePath = "navbanner";
        private readonly ILanguageService _languageService;
        private readonly INavBannerImageRepository _navBannerImageRepository;
        private readonly INavBannerLinkRepository _navBannerLinkRepository;
        private readonly INavBannerLinkTextRepository _navBannerLinkTextRepository;
        private readonly INavBannerRepository _navBannerRepository;
        private readonly ISiteSettingService _siteSettingService;

        public NavBannerService(ILogger<NavBannerService> logger,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            INavBannerImageRepository navBannerImageRepository,
            INavBannerLinkRepository navBannerLinkRepository,
            INavBannerLinkTextRepository navBannerLinkTextRepository,
            INavBannerRepository navBannerRepository,
            ISiteSettingService siteSettingService)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerImageRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkTextRepository);
            ArgumentNullException.ThrowIfNull(navBannerRepository);
            ArgumentNullException.ThrowIfNull(siteSettingService);

            _languageService = languageService;
            _navBannerImageRepository = navBannerImageRepository;
            _navBannerLinkRepository = navBannerLinkRepository;
            _navBannerLinkTextRepository = navBannerLinkTextRepository;
            _navBannerRepository = navBannerRepository;
            _siteSettingService = siteSettingService;
        }

        public async Task AddImageNoSaveAsync(NavBannerImage image)
        {
            ArgumentNullException.ThrowIfNull(image);

            image.ImageAltText = image.ImageAltText?.Trim();

            await _navBannerImageRepository.AddAsync(image);
        }

        public async Task AddLinkAsync(NavBannerLink navBannerLink)
        {
            await _navBannerLinkRepository.AddAsync(navBannerLink);
            await _navBannerLinkRepository.SaveAsync();
        }

        public async Task AddLinkTextAsync(NavBannerLinkText navBannerLinkText)
        {
            await _navBannerLinkTextRepository.AddAsync(navBannerLinkText);
            await _navBannerLinkTextRepository.SaveAsync();
        }

        public async Task<NavBanner> CloneAsync(int navBannerId)
        {
            var navBanner = await _navBannerRepository.GetByIdAsync(navBannerId)
                ?? throw new OcudaException($"No NavBanner found for id {navBannerId}");

            navBanner.Id = 0;

            await _navBannerRepository.AddAsync(navBanner);

            var navBannerImages = await _navBannerImageRepository
                .GetAllByNavBannerIdAsync(navBannerId);

            foreach (var image in navBannerImages)
            {
                image.NavBannerId = 0;
                image.NavBanner = navBanner;

                await _navBannerImageRepository.AddAsync(image);
            }

            var navBannerLinks = await _navBannerLinkRepository
                .GetLinksByNavBannerIdAsync(navBannerId);

            if (navBannerLinks.Count > 0)
            {
                foreach (var link in navBannerLinks)
                {
                    var linkTexts = await _navBannerLinkTextRepository
                        .GetAllLanguageTextsAsync(link.Id);

                    link.Id = 0;
                    link.NavBanner = navBanner;
                    link.NavBannerId = 0;

                    foreach (var text in linkTexts)
                    {
                        text.NavBannerLink = link;
                        text.NavBannerLinkId = 0;
                    }

                    await _navBannerLinkTextRepository.AddRangeAsync(linkTexts);
                }

                await _navBannerLinkRepository.AddRangeAsync(navBannerLinks);
            }

            return navBanner;
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

        public async Task<NavBanner> GetByIdAsync(int navBannerId)
        {
            return await _navBannerRepository.GetByIdAsync(navBannerId);
        }

        public async Task<NavBannerImage> GetImageByNavBannerIdAsync(int navBannerId,
            int languageId)
        {
            var navBannerImage = await _navBannerImageRepository
                .GetByNavBannerIdAsync(navBannerId, languageId);

            if (navBannerImage != null)
            {
                var language = await _languageService.GetActiveByIdAsync(languageId);

                if (language != null)
                {
                    var imagePath = await GetFullImageDirectoryPath(_siteSettingService,
                        language.Name,
                        NavBannerFilePath);

                    navBannerImage.ImageFilePath = Path.Combine(imagePath, navBannerImage.Filename);
                }
            }
            return navBannerImage;
        }

        public async Task<ICollection<NavBannerLink>> GetLinksByNavBannerIdAsync(int navBannerId,
            int languageId)
        {
            var links = await _navBannerLinkRepository.GetLinksByNavBannerIdAsync(navBannerId);

            if (links?.Count > 0)
            {
                foreach (var link in links)
                {
                    link.NavBannerLinkText = await _navBannerLinkTextRepository
                        .FindAsync(link.Id, languageId);
                }
            }

            return links;
        }

        public async Task<int?> GetPageHeaderIdAsync(int navBannerId)
        {
            return await _navBannerRepository.GetPageHeaderIdAsync(navBannerId);
        }

        public async Task<int?> GetPageLayoutIdForNavBannerAsync(int id)
        {
            return await _navBannerRepository.GetPageLayoutIdAsync(id);
        }

        public async Task<string> GetUploadImageFilePathAsync(string languageName)
        {
            return await GetFullImageDirectoryPath(_siteSettingService,
                languageName,
                NavBannerFilePath);
        }

        public async Task<int> ImageUseCountAsync(int languageId, string navBannerImageFilename)
        {
            return await _navBannerImageRepository.CountAsync(languageId, navBannerImageFilename);
        }

        public void UpdateImageNoSave(NavBannerImage image)
        {
            _navBannerImageRepository.Update(image);
        }

        public async Task UpdateLinkAsync(NavBannerLink navBannerLink)
        {
            ArgumentNullException.ThrowIfNull(navBannerLink);
            var existingRecord = await _navBannerLinkRepository.FindAsync(navBannerLink.Id);
            existingRecord.Icon = navBannerLink.Icon?.Trim();
            existingRecord.Link = navBannerLink.Link?.Trim();
            _navBannerLinkRepository.Update(existingRecord);
            await _navBannerImageRepository.SaveAsync();
        }

        public async Task UpdateLinkTextAsync(NavBannerLinkText navBannerLinkText)
        {
            ArgumentNullException.ThrowIfNull(navBannerLinkText);
            var existingRecord = await _navBannerLinkTextRepository
                .FindAsync(navBannerLinkText.NavBannerLinkId, navBannerLinkText.LanguageId);
            existingRecord.Text = navBannerLinkText.Text?.Trim();
            _navBannerLinkTextRepository.Update(existingRecord);
            await _navBannerLinkTextRepository.SaveAsync();
        }
    }
}