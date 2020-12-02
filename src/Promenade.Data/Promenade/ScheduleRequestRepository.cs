using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

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
    }
}
