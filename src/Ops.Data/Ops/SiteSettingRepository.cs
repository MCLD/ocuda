using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SiteSettingRepository
        : OpsRepository<OpsContext, SiteSetting, string>, ISiteSettingRepository
    {
        public SiteSettingRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SiteSettingRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
