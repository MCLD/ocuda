using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class NavBannerService : BaseService<NavBannerService>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LanguageService _languageService;
        private readonly INavBannerRepository _navBannerRepository;
        private readonly INavBannerImageRepository _navBannerImageRepository;
        private readonly INavBannerLinkRepository _navBannerLinkRepository;
        private readonly INavBannerLinkTextRepository _navBannerLinkTextRepository;

        public NavBannerService(IHttpContextAccessor httpContextAccessor,
            LanguageService languageService,
            ILogger<NavBannerService> logger,
            IDateTimeProvider dateTimeProvider,
            INavBannerRepository navBannerRepository,
            INavBannerImageRepository navBannerImageRepository,
            INavBannerLinkRepository navBannerLinkRepository,
            INavBannerLinkTextRepository navBannerLinkTextRepository) : base(logger, dateTimeProvider)
        {
            ArgumentNullException.ThrowIfNull(httpContextAccessor);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(navBannerRepository);
            ArgumentNullException.ThrowIfNull(navBannerImageRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkRepository);
            ArgumentNullException.ThrowIfNull(navBannerLinkTextRepository);

            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _navBannerRepository = navBannerRepository;
            _navBannerImageRepository = navBannerImageRepository;
            _navBannerLinkRepository = navBannerLinkRepository;
            _navBannerLinkTextRepository = navBannerLinkTextRepository;
        }

        public async Task<NavBanner> GetNavBannerAsync(int navBannerId)
        {
            return await GetIncludingChildrenAsync(navBannerId);
        }
        public async Task<NavBanner> GetIncludingChildrenAsync(int navBannerId)
        {
            var navBanner = await _navBannerRepository.GetByIdAsync(navBannerId);

            var languageIds
                = await GetCurrentDefaultLanguageIdAsync(
                    _httpContextAccessor,
                    _languageService);

            var languageId = languageIds.FirstOrDefault();
            
            navBanner.NavBannerImage = await _navBannerImageRepository.GetByNavBannerIdAsync(navBannerId, languageId);

            var navBannerLinks = await _navBannerLinkRepository.GetByNavBannerIdAsync(navBannerId);

            foreach ( var link in navBannerLinks )
            {
                link.Text = await _navBannerLinkTextRepository.GetByLinkIdAsync(link.Id, languageId);
            }

            navBanner.NavBannerLinks = navBannerLinks;

            return navBanner;
        }
    }
}
