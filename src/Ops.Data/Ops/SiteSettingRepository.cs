using Microsoft.Extensions.Logging;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository : GenericRepository<Models.SiteSetting, int>
    {
        public SiteSettingRepository(OpsContext context, ILogger<SiteSettingRepository> logger)
            : base(context, logger)
        {
        }
    }
}
