﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class GroupRepository : GenericRepository<PromenadeContext, Group, int>, IGroupRepository
    {
        public GroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<GroupRepository> logger) : base(repositoryFacade, logger)
        {
        }
        public async Task<List<Group>> GetAllGroups()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.GroupType)
                .ToListAsync();
        }
    }
}
