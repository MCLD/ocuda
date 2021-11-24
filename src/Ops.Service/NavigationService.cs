using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service
{
    public class NavigationService : BaseService<NavigationService>, INavigationService
    {
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;
        private readonly ISiteSettingPromService _siteSettingPromService;

        public NavigationService(ILogger<NavigationService> logger,
            IHttpContextAccessor httpContextAccessor,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository,
            ISiteSettingPromService siteSettingPromService)
            : base(logger, httpContextAccessor)
        {
            _navigationRepository = navigationRepository 
                ?? throw new ArgumentNullException(nameof(navigationRepository));
            _navigationTextRepository = navigationTextRepository
                ?? throw new ArgumentNullException(nameof(navigationTextRepository));
            _siteSettingPromService = siteSettingPromService
                ?? throw new ArgumentNullException(nameof(siteSettingPromService));
        }

        public async Task<ICollection<Navigation>> GetTopLevelNavigationsAsync()
        {
            return await _navigationRepository.GetTopLevelNavigationsAsync();
        }

        public async Task<Navigation> CreateAsync(Navigation navigation)
        {
            navigation.Icon = navigation.Icon?.Trim();
            navigation.Name = navigation.Name?.Trim();

            await _navigationRepository.AddAsync(navigation);
            await _navigationRepository.SaveAsync();
            return navigation;
        }
    }
}
