using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Data.ServiceFacade;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Data.Promenade
{
    public class ExternalResourceRepository
        : GenericRepository<PromenadeContext, ExternalResource, int>, IExternalResourceRepository
    {
        public ExternalResourceRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<ExternalResourceRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type)
        {
            var externalResources = DbSet
                .AsNoTracking();

            if (type.HasValue)
            {
                externalResources = externalResources
                    .Where(_ => _.Type == type.Value);
            }

            return await externalResources
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }
    }
}
