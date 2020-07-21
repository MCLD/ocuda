using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaRepository
        : GenericRepository<PromenadeContext, Emedia>, IEmediaRepository
    {
        public EmediaRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Emedia>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }
    }
}
