using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduleRequestLimitRepository
        : GenericRepository<PromenadeContext, ScheduleRequestLimit>, IScheduleRequestLimitRepository
    {
        public ScheduleRequestLimitRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestLimitRepository> logger) : base(repositoryFacade, logger)
        {
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
