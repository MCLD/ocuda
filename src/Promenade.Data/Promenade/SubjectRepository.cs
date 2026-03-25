using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SubjectRepository(
        ServiceFacade.Repository<PromenadeContext> repositoryFacade,
        ILogger<SubjectRepository> logger)
                : GenericRepository<PromenadeContext, Subject>(repositoryFacade, logger), ISubjectRepository
    {
        public async Task<ICollection<Subject>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Id)
                .ToListAsync();
        }
    }
}