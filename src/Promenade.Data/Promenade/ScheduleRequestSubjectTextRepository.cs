using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ScheduleRequestSubjectTextRepository
        : GenericRepository<PromenadeContext, ScheduleRequestSubjectText>,
        IScheduleRequestSubjectTextRepository
    {
        public ScheduleRequestSubjectTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<ScheduleRequestSubjectTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<string> GetByIdsAsync(int scheduleRequestSubjectId,
            int languageId)
        {
            var item = await DbSet
                .AsNoTracking()
                .Where(_ => _.ScheduleRequestSubjectId == scheduleRequestSubjectId
                    && _.LanguageId == languageId)
                .SingleOrDefaultAsync();

            return item?.SubjectText;
        }
    }
}