using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ScheduleRequestLimitRepository
        : GenericRepository<PromenadeContext, ScheduleRequestLimit>, IScheduleRequestLimitRepository
    {
        public ScheduleRequestLimitRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestLimitRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int?> GetTimeSlotLimitAsync(DateTime requestTime)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DayOfWeek == requestTime.DayOfWeek && _.Hour == requestTime.Hour)
                .Select(_ => (int?)_.Limit)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<ScheduleRequestLimit>> GetLimitsForHourAsync(int hour)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Hour == hour)
                .ToListAsync();
        }

        public async Task<ICollection<ScheduleRequestLimit>> GetLimitsForDayAsync(DayOfWeek day)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.DayOfWeek == day)
                .ToListAsync();
        }
    }
}
