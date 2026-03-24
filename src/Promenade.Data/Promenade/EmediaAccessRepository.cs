using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaAccessRepository(
        ServiceFacade.Repository<PromenadeContext> repositoryFacade,
        ILogger<UrlRedirectRepository> logger)
            : GenericRepository<PromenadeContext, EmediaAccess>(repositoryFacade, logger),
            IEmediaAccessRepository
    {
        public async Task AddAccessLogAsync(int emediaId)
        {
            await DbSet.AddAsync(new EmediaAccess
            {
                EmediaId = emediaId,
                AccessDate = _dateTimeProvider.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}