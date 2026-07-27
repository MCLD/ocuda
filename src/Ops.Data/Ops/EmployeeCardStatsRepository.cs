using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class EmployeeCardStatsRepository(Repository<OpsContext> repositoryFacade,
        ILogger<EmployeeCardStatsRepository> logger)
        : GenericRepository<OpsContext, EmployeeCardStats>(repositoryFacade, logger),
        IEmployeeCardStatsRepository
    {
        public async Task<bool> AnyAsync()
        {
            return await DbSet.AsNoTracking().AnyAsync();
        }

        public async Task<int> GetDateCountAsync(BaseFilter<string> filter)
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Year)
                .Distinct()
                .CountAsync();
        }

        public async Task<IDictionary<DateTime, int?>> GetDatesAsync(BaseFilter<string> filter)
        {
            return await DbSet
                .AsNoTracking()
                .GroupBy(_ => _.Year)
                .Select(_ => new
                {
                    _.Key,
                    Count = _.Count(),
                })
                .OrderByDescending(_ => _.Key)
                .ApplyPagination(filter)
                .ToDictionaryAsync(k => new DateTime(k.Key, 1, 1), v => (int?)v.Count);
        }

        public async Task<DateTime?> GetLatestDateAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderByDescending(_ => _.Year)
                .ThenByDescending(_ => _.Month)
                .Take(1)
                .Select(_ => new DateTime(_.Year, _.Month, 1))
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<EmployeeCardStats>> GetReportAsync(DateTime period)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Year == period.Year)
                .ToListAsync();
        }

        public async Task SaveStatsAsync(EmployeeCardStats stats)
        {
            await DbSet.AddAsync(stats);
            await _context.SaveChangesAsync();
        }
    }
}
