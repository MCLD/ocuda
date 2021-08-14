using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaTextRepository 
        : GenericRepository<PromenadeContext, EmediaText>, IEmediaTextRepository
    {
        public EmediaTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<EmediaText> GetByEmediaAndLanguageAsync(int emediaId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<EmediaText>> GetAllForGroupAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.GroupId == groupId)
                .ToListAsync();
        }
    }
}
