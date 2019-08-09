using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Ops
{
    public class GroupRepository : GenericRepository<PromenadeContext,Group, int>, IGroupRepository
    {
        public GroupRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Group>> GetAllGroupsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.GroupType)
                .ToListAsync();
        }
        public async Task<List<Group>> GetAllGroupRegions()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsLocationRegion)
                .OrderBy(_ => _.GroupType)
                .ToListAsync();
        }
        public async Task<List<Group>> GetAllNonGroupRegions()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsLocationRegion)
                .OrderBy(_ => _.GroupType)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Group>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<Group>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.GroupType)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> IsDuplicateGroupTypeAsync(Group group)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.GroupType.ToLower() == group.GroupType.ToLower())
                .AnyAsync();
        }
        public async Task<bool> IsDuplicateIdAsync(Group group)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != group.Id)
                .AnyAsync();
        }
    }
}
