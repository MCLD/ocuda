using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class ScheduleRequestSubjectRepository
        : GenericRepository<PromenadeContext, ScheduleRequestSubject>,
        IScheduleRequestSubjectRepository
    {
        public ScheduleRequestSubjectRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestSubjectRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<ScheduleRequestSubject>> GetUsingSegmentAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId)
                .ToListAsync();
        }
    }
}
