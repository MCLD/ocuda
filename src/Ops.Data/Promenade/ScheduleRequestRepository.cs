using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduleRequestRepository : GenericRepository<PromenadeContext, ScheduleRequest>,
        IScheduleRequestRepository

    {
        public ScheduleRequestRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task<CollectionWithCount<ScheduleRequest>> GetPaginatedAsync(ScheduleRequestFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter?.IsCancelled == true)
            {
                query = query.Where(_ => _.IsCancelled);
            }
            return new CollectionWithCount<ScheduleRequest>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .Include(_ => _.ScheduleRequestSubject)
                    .OrderByDescending(_ => _.RequestedTime)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<ICollection<ScheduleRequest>> GetPendingNotificationsAsync()
        {
            return await DbSet
                .Include(_ => _.ScheduleRequestSubject)
                .Where(_ => _.NotificationSentAt == null
                    && _.Email != null
                    && _.ScheduleRequestSubject.RelatedEmailSetupId != null)
                .OrderBy(_ => _.RequestedTime)
                .ToListAsync();
        }

        public async Task<ScheduleRequest> GetRequestAsync(int requestId)
        {
            return await DbSet
                .Include(_ => _.ScheduleRequestSubject)
                .Include(_ => _.ScheduleRequestTelephone)
                .Where(_ => _.Id == requestId)
                .AsNoTracking()
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<ScheduleRequest>> GetRequestsAsync(DateTime requestedDate)
        {
            return await DbSet
                .Include(_ => _.ScheduleRequestSubject)
                .Where(_ => _.RequestedTime.Date == requestedDate.Date)
                .OrderBy(_ => _.RequestedTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ScheduleRequest>> GetUnclaimedRequestsAsync()
        {
            return await DbSet
                .Include(_ => _.ScheduleRequestSubject)
                .Where(_ => !_.IsClaimed)
                .OrderBy(_ => _.RequestedTime)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}