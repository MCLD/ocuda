using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaTextRepository> logger)
            : GenericRepository<PromenadeContext, EmediaText>(repositoryFacade, logger),
            IEmediaTextRepository
    {
        public async Task<ICollection<EmediaText>> GetAllForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.EmediaId == emediaId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaText>> GetAllForGroupAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.Emedia.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<EmediaText> GetByEmediaAndLanguageAsync(int emediaId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive
                    && _.EmediaId == emediaId
                    && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<string>> GetUsedLanguagesForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.IsActive && _.EmediaId == emediaId)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }
    }
}