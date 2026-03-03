using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaSubjectRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaSubjectRepository> logger)
            : GenericRepository<PromenadeContext, EmediaSubject>(repositoryFacade, logger),
            IEmediaSubjectRepository
    {
        public async Task<int[]> GetEmediaIdsAsync(int subjectId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SubjectId == subjectId)
                .Select(_ => _.EmediaId)
                .Distinct()
                .ToArrayAsync();
        }
    }
}