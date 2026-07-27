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
    public class EmediaStatsRepository(Repository<OpsContext> repositoryFacade,
        ILogger<EmediaStatsRepository> logger)
        : GenericRepository<OpsContext, EmediaStats>(repositoryFacade, logger),
        IEmediaStatsRepository
    {
        public async Task<bool> AnyAsync()
        {
            return await DbSet.AsNoTracking().AnyAsync();
        }

        public async Task<int> GetDateCountAsync(BaseFilter<string> filter)
        {
            return await DbSet
                .AsNoTracking()
                .GroupBy(_ => new { _.Year, _.Month })
                .CountAsync();
        }

        public async Task<IDictionary<DateTime, int?>> GetDatesAsync(BaseFilter<string> filter)
        {
            return await DbSet
                .AsNoTracking()
                .GroupBy(_ => new { _.Year, _.Month })
                .Select(_ => new
                {
                    _.Key,
                    Sum = _.Sum(__ => __.Accesses),
                })
                .OrderByDescending(_ => _.Key.Year)
                .ThenBy(_ => _.Key.Month)
                .ApplyPagination(filter)
                .ToDictionaryAsync(k => new DateTime(k.Key.Year, k.Key.Month, 1),
                    v => (int?)v.Sum);
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

        public async Task<IEnumerable<EmediaStats>> GetReportAsync(DateTime period)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Year == period.Year && _.Month == period.Month)
                .ToListAsync();
        }

        public async Task SaveStatsAsync(IEnumerable<EmediaStats> stats)
        {
            await DbSet.AddRangeAsync(stats);
            await _context.SaveChangesAsync();
        }
    }
}
