using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class EmployeeCardResultRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<EmployeeCardResultRepository> logger)
        : OpsRepository<OpsContext, EmployeeCardResult, int>(repositoryFacade, logger),
        IEmployeeCardResultRepository
    {
        public async Task<IEnumerable<DateTime>> GetDatesAfterAsync(DateTime afterMonthYear)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ProcessedAt > afterMonthYear.AddMonths(1))
                .Select(_ => new DateTime(_.ProcessedAt.Value.Year, _.ProcessedAt.Value.Month, 1))
                .Distinct()
                .ToListAsync();
        }

        public async Task<CollectionWithCount<EmployeeCardResult>> GetPaginatedAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new CollectionWithCount<EmployeeCardResult>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.SubmittedAt)
                    .ApplyPagination(filter)
                    .ToListAsync(),
            };
        }

        public async Task<ICollection<EmployeeCardResult>> GetStatsAsync(DateTime monthYear)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ProcessedAt.HasValue
                    && _.ProcessedAt.Value.Year == monthYear.Year
                    && _.ProcessedAt.Value.Month == monthYear.Month)
                .ToListAsync();
        }
    }
}
