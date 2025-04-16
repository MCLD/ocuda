using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class PermissionGroupRepository
        : OpsRepository<OpsContext, PermissionGroup, int>, IPermissionGroupRepository
    {
        public PermissionGroupRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<PermissionGroup>> GetAllAsync()
        {
            return await DbSet.AsNoTracking().ToListAsync();
        }

        public async Task<ICollection<PermissionGroup>>
            GetGroupsAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => permissionGroupIds.Contains(_.Id))
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<PermissionGroup>>>
            GetPaginatedListAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<PermissionGroup>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.PermissionGroupName)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> IsDuplicateAsync(PermissionGroup permissionGroup)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != permissionGroup.Id
                    && _.PermissionGroupName == permissionGroup.PermissionGroupName)
                .AnyAsync();
        }

        public override async Task<ICollection<PermissionGroup>> ToListAsync(
            params Expression<Func<PermissionGroup, IComparable>>[] orderBys)
        {
            if (orderBys == null || orderBys.Length == 0)
            {
                throw new ArgumentNullException(nameof(orderBys));
            }

            return await DbSetOrdered(orderBys).AsNoTracking().ToListAsync();
        }
    }
}