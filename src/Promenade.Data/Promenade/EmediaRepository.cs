using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmediaRepository(
        ServiceFacade.Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaRepository> logger)
            : GenericRepository<PromenadeContext, Emedia>(repositoryFacade, logger),
            IEmediaRepository
    {
        public async Task<ICollection<Emedia>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }
    }
}