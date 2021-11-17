using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Service
{
    public class NavigationService : BaseService<NavigationService>, INavigationService
    {
        private readonly INavigationRepository _navigationRepository;
        private readonly INavigationTextRepository _navigationTextRepository;

        public NavigationService(ILogger<NavigationService> logger,
            IHttpContextAccessor httpContextAccessor,
            INavigationRepository navigationRepository,
            INavigationTextRepository navigationTextRepository)
            : base(logger, httpContextAccessor)
        {
            _navigationRepository = navigationRepository 
                ?? throw new ArgumentNullException(nameof(navigationRepository));
            _navigationTextRepository = navigationTextRepository
                ?? throw new ArgumentNullException(nameof(navigationTextRepository));
        }
    }
}
