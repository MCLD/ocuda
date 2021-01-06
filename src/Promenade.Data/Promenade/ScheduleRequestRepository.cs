using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ScheduleRequestRepository
        : GenericRepository<PromenadeContext, ScheduleRequest>, IScheduleRequestRepository
    {
        public ScheduleRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ScheduleRequest> AddSaveAsync(ScheduleRequest scheduleRequest)
        {
            var addedRequest = await DbSet.AddAsync(scheduleRequest);
            await _context.SaveChangesAsync();
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == addedRequest.Entity.Id);
        }

        public async Task<int> GetTimeSlotCountAsync(DateTime requestTime)
        {
            return await DbSet
                .AsNoTracking()
                .CountAsync(_ => !_.IsCancelled
                    && _.RequestedTime.Date == requestTime.Date
                    && _.RequestedTime.Hour == requestTime.Hour);
        }

        public async Task<ICollection<DataWithCount<int>>> GetDayRequestCountsAsync(
            DateTime requestedTime,
            DateTime firstAvailable)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsCancelled
                    && _.RequestedTime.Date == requestedTime.Date
                    && (_.RequestedTime.Date > firstAvailable.Date
                        || _.RequestedTime.Hour >= firstAvailable.Hour))
                .GroupBy(_ => _.RequestedTime.Hour)
                .Select(_ => new DataWithCount<int>
                {
                    Count = _.Count(),
                    Data = _.Key
                })
                .ToListAsync();
        }

        public async Task<ICollection<DataWithCount<DateTime>>> GetHourRequestCountsAsync(
            DateTime requestedTime,
            DateTime firstAvailable)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsCancelled
                    && _.RequestedTime.Hour == requestedTime.Hour
                    && _.RequestedTime.Date >= firstAvailable.Date
                    && _.RequestedTime.Date >= requestedTime.Date
                        .AddDays(-Enum.GetNames(typeof(DayOfWeek)).Length)
                    && _.RequestedTime.Date <= requestedTime.Date
                        .AddDays(Enum.GetNames(typeof(DayOfWeek)).Length))
                .GroupBy(_ => _.RequestedTime.Date)
                .Select(_ => new DataWithCount<DateTime>
                {
                    Count = _.Count(),
                    Data = _.Key
                })
                .ToListAsync();
        }
    }
}
