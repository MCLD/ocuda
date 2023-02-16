using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class UserSyncLocationRepository
        : OpsRepository<OpsContext, UserSyncLocation, int>, IUserSyncLocationRepository
    {
        public UserSyncLocationRepository(Repository<OpsContext> repositoryFacade,
            ILogger<UserSyncLocationRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<UserSyncLocation>> GetAllAsync()
        {
            return await DbSet
                 .AsNoTracking()
                 .OrderBy(_ => _.Name)
                 .ToListAsync();
        }
    }
}