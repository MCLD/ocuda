using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SubjectTextRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<SubjectTextRepository> logger)
            : GenericRepository<PromenadeContext, SubjectText>(repositoryFacade, logger),
            ISubjectTextRepository
    {
        public async Task<SubjectText> GetByIdsAsync(int subjectId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SubjectId == subjectId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}