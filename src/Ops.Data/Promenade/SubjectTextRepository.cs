using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class SubjectTextRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<SubjectTextRepository> logger)
            : GenericRepository<PromenadeContext, SubjectText>(repositoryFacade, logger),
            ISubjectTextRepository
    {
        public async Task<ICollection<SubjectText>> GetAllForSubjectAsync(int subjectId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SubjectId == subjectId)
                .ToListAsync();
        }

        public async Task<SubjectText> GetBySubjectAndLanguageAsync(int subjectId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SubjectId == subjectId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<string>> GetUsedLanguagesForSubjectAsync(int subjectId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SubjectId == subjectId)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }
    }
}