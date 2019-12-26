using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SiteSettingRepository
        : GenericRepository<PromenadeContext, SiteSetting, string>, ISiteSettingRepository
    {
        public SiteSettingRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SiteSettingRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
