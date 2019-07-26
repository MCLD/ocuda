using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class UrlRedirectRepository 
        : GenericRepository<PromenadeContext, UrlRedirect, int>, IUrlRedirectRepository
    {
        public UrlRedirectRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<UrlRedirectRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<UrlRedirect> GetRedirectByPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.RequestPath == path)
                .SingleOrDefaultAsync();
        }
    }
}
