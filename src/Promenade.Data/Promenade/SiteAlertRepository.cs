using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SiteAlertRepository
        : GenericRepository<PromenadeContext, SiteAlert, int>, ISiteAlertRepository
    {
        public SiteAlertRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SiteAlertRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<SiteAlert>> GetCurrentSiteAlertsAsync()
        {
            var now = _dateTimeProvider.Now;

            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.StartAt >= now && _.EndAt <= now)
                .OrderByDescending(_ => _.StartAt)
                .ToListAsync();
        }
    }
}
