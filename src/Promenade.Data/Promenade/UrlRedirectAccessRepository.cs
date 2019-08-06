using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class UrlRedirectAccessRepository
        : GenericRepository<PromenadeContext, UrlRedirectAccess, int>, IUrlRedirectAccessRepository
    {
        public UrlRedirectAccessRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<UrlRedirectRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddAccessLogAsync(int redirectId)
        {
            await DbSet.AddAsync(new UrlRedirectAccess
            {
                UrlRedirectId = redirectId,
                AccessDate = _dateTimeProvider.Now
            });
            await _context.SaveChangesAsync();
        }
    }
}
