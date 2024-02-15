using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Abstract;

namespace Ocuda.Ops.Service
{
    public class NavBannerService : BaseService<NavBannerService>, INavBannerService
    {
        public NavBannerService(ILogger<NavBannerService> logger,
            IHttpContextAccessor httpContextAccessor
            ) : base(logger, httpContextAccessor)
        { 
        }
    }
}
