using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaAccessRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaAccessRepository> logger)
        : GenericRepository<PromenadeContext, EmediaAccess>(repositoryFacade, logger),
        IEmediaAccessRepository
    {
        public async Task<IEnumerable<DateTime>> GetDatesAfterAsync(DateTime afterMonthYear)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.AccessDate > afterMonthYear.AddMonths(1))
                .Select(_ => new DateTime(_.AccessDate.Year, _.AccessDate.Month, 1))
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<EmediaStats>> GetStatsAsync(DateTime monthYear)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.AccessDate.Year == monthYear.Year
                    && _.AccessDate.Month == monthYear.Month)
                .GroupBy(_ => _.EmediaId)
                .Select(_ => new EmediaStats
                {
                    Accesses = _.Count(),
                    EmediaId = _.Key,
                })
                .ToListAsync();
        }
    }
}
