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
    public class ScheduleRequestSubjectRepository
        : GenericRepository<PromenadeContext, ScheduleRequestSubject>,
        IScheduleRequestSubjectRepository
    {
        public ScheduleRequestSubjectRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestSubjectRepository> logger) : base(repositoryFacade, logger)
        {

        }

        public async Task<IEnumerable<ScheduleRequestSubject>> GetAllAsync()
        {
            return await DbSet.OrderBy(_ => _.Subject).ToListAsync();
        }
    }
}
