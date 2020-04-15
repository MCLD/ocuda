using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ScheduleRequestTelephoneRepository
        : GenericRepository<PromenadeContext, ScheduleRequestTelephone>,
        IScheduleRequestTelephoneRepository
    {
        public ScheduleRequestTelephoneRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestTelephoneRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task<ScheduleRequestTelephone> AddSaveAsync(string phone)
        {
            var addedRequest = await DbSet.AddAsync(new ScheduleRequestTelephone
            {
                Phone = phone
            });
            await _context.SaveChangesAsync();
            return addedRequest.Entity;
        }

        public async Task<ScheduleRequestTelephone> GetAsync(string phone)
        {
            return await DbSet.AsNoTracking().SingleOrDefaultAsync(_ => _.Phone == phone);
        }
    }
}
