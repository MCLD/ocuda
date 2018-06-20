using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository 
        : GenericRepository<Models.SiteSetting, int>, ISiteSettingRepository
    {
        public SiteSettingRepository(OpsContext context, ILogger<SiteSettingRepository> logger)
            : base(context, logger)
        {
        }
    }
}
