using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RenewCardResultRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<RenewCardResultRepository> logger)
        : OpsRepository<OpsContext, RenewCardResult, int>(repositoryFacade, logger),
        IRenewCardResultRepository
    {
        public async Task<IEnumerable<DateTime>> GetDatesAfterAsync(DateTime afterMonthYear)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CreatedAt > afterMonthYear.AddMonths(1))
                .Select(_ => new DateTime(_.CreatedAt.Year, _.CreatedAt.Month, 1))
                .Distinct()
                .ToListAsync();
        }

        public async Task<RenewCardResult> GetForRequestAsync(int requestId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.CreatedByUser)
                .Where(_ => _.RenewCardRequestId == requestId)
                .SingleOrDefaultAsync();
        }

        public async Task<RenewCardResponse.ResponseType> GetRequestResponseTypeAsync(
            int requestId)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.RenewCardRequestId == requestId)
                .Select(_ => _.RenewCardResponse.Type)
                .SingleOrDefaultAsync();
        }

        public async Task<RenewCardResponseStats> GetStatsAsync(DateTime monthYear)
        {
            var stats = new RenewCardResponseStats
            {
                DiscardedCount = await DbSet
                    .AsNoTracking()
                    .CountAsync(_ => _.CreatedAt.Year == monthYear.Year
                        && _.CreatedAt.Month == monthYear.Year
                        && _.IsDiscarded),
            };

            var statsDictionary = await DbSet
                .AsNoTracking()
                .Where(_ => _.CreatedAt.Year == monthYear.Year
                    && _.CreatedAt.Month == monthYear.Month
                    && !_.IsDiscarded)
                .GroupBy(_ => _.RenewCardResponseId)
                .ToDictionaryAsync(k => k.Key, v => v.Count());

            foreach (var key in statsDictionary.Keys)
            {
                if (!key.HasValue)
                {
                    stats.NotProcessedCount = statsDictionary[key];
                }
                else
                {
                    stats.ResponseIdCount.Add(key.Value, statsDictionary[key]);
                }
            }

            return stats;
        }
    }
}
