using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaCategoryRepository
        : GenericRepository<PromenadeContext, EmediaCategory>, IEmediaCategoryRepository
    {
        public EmediaCategoryRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaCategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<EmediaCategory>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.EmediaId)
                .ThenBy(_ => _.CategoryId)
                .ToListAsync();
        }
    }
}
