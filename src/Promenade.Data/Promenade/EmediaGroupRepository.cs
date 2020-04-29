using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaGroupRepository
        : GenericRepository<PromenadeContext, EmediaGroup>, IEmediaGroupRepository
    {
        public EmediaGroupRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<EmediaGroup>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }
    }
}
