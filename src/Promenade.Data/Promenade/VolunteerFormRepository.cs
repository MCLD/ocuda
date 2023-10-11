using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class VolunteerFormRepository : GenericRepository<PromenadeContext, VolunteerForm>, 
        IVolunteerFormRepository
    {
        public VolunteerFormRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<VolunteerFormRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<VolunteerForm>> FindAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDisabled)
                .ToListAsync();
        }

        public async Task<VolunteerForm> FindByTypeAsync(VolunteerFormType type)
        {
            return await DbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(_ => _.VolunteerFormType == type);
        }
    }
}