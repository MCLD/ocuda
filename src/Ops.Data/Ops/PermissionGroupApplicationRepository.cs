using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class PermissionGroupApplicationRepository
        : OpsRepository<OpsContext, PermissionGroupApplication, int>,
        IPermissionGroupApplicationRepository
    {
        public PermissionGroupApplicationRepository(
            ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupApplication> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<int> GetApplicationPermissionGroupCountAsync(string permission)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ApplicationPermission == permission)
                .CountAsync();
        }

        public async Task<ICollection<PermissionGroup>>
            GetApplicationPermissionGroupsAsync(string permission)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ApplicationPermission == permission)
                .Select(_ => _.PermissionGroup)
                .ToListAsync();
        }

        public async Task<ICollection<string>> GetAssignedPermissions(int permissionGroupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PermissionGroupId == permissionGroupId)
                .Select(_ => _.ApplicationPermission)
                .ToListAsync();
        }
    }
}