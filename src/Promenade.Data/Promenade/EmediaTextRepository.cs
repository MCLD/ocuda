using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaTextRepository
        : GenericRepository<PromenadeContext, EmediaText>, IEmediaTextRepository
    {
        public EmediaTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<EmediaText> GetByIdsAsync(int emediaId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId && _.LanguageId == languageId)
                .SingleOrDefaultAsync();
        }
    }
}