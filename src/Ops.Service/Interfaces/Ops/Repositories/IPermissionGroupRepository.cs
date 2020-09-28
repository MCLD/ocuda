using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupRepository : IOpsRepository<PermissionGroup, int>
    {
        Task<DataWithCount<ICollection<PermissionGroup>>> GetPaginatedListAsync(BaseFilter filter);
        new Task<ICollection<PermissionGroup>>
            ToListAsync(params Expression<Func<PermissionGroup, IComparable>>[] orderBys);
        Task<bool> IsDuplicateAsync(PermissionGroup permissionGroup);
        Task<ICollection<PermissionGroup>> GetAllAsync();
        Task<ICollection<PermissionGroup>> GetGroupsAsync(int[] permissionGroupIds);
    }
}
