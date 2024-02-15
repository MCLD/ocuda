using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using System;

namespace Ocuda.Ops.Service
{
    public class NavBannerService : BaseService<NavBannerService>, INavBannerService
    {
        private readonly INavBannerRepository _navBannerRepository;
        public NavBannerService(ILogger<NavBannerService> logger,
            IHttpContextAccessor httpContextAccessor,
            INavBannerRepository navBannerRepository
            ) : base(logger, httpContextAccessor)
        { 
            _navBannerRepository = navBannerRepository;
        }

        public async Task<NavBanner> CreateNoSaveAsync(NavBanner navBanner)
        {
            navBanner.Name = navBanner.Name?.Trim();

            await _navBannerRepository.AddAsync(navBanner);
            return navBanner;
        }

        public async Task EditAsync(NavBanner navBanner)
        {
            if (navBanner == null)
            {
                throw new ArgumentNullException(nameof(navBanner));
            }

            var updateNavBanner = await _navBannerRepository.GetByIdAsync(navBanner.Id);
            if (navBanner != null)
            {
                updateNavBanner.Name = navBanner.Name;
            }
            _navBannerRepository.Update(updateNavBanner);
            await _navBannerRepository.SaveAsync();
        }
    }
}
