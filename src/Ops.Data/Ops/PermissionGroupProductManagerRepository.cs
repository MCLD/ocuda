using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Data.Ops
{
    public class PermissionGroupProductManagerRepository
        : GenericRepository<OpsContext, PermissionGroupProductManager>,
            IPermissionGroupProductManagerRepository

    {
        public PermissionGroupProductManagerRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupProductManagerRepository> logger) : base(repositoryFacade, logger)
        { }

        public async Task AddSaveAsync(int productId, int permissionGroupId)
        {
            await DbSet.AddAsync(new PermissionGroupProductManager
            {
                ProductId = productId,
                PermissionGroupId = permissionGroupId
            });
            await SaveAsync();
        }

        public async Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => permissionGroupIds.Contains(_.PermissionGroupId))
                .AnyAsync();
        }

        public async Task<ICollection<PermissionGroupProductManager>>
            GetByPermissionGroupIdAsync(int permissionGroupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PermissionGroupId == permissionGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<int>>
            GetByPermissionGroupIdsAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .Where(_ => permissionGroupIds.Contains(_.PermissionGroupId))
                .AsNoTracking()
                .Select(_ => _.ProductId)
                .ToListAsync();
        }

        public async Task<ICollection<PermissionGroupProductManager>>
            GetByProductIdAsync(int productId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ProductId == productId)
                .ToListAsync();
        }

        public async Task RemoveSaveAsync(int productId, int permissionGroupId)
        {
            var item = await DbSet.SingleOrDefaultAsync(_ => _.ProductId == productId
                && _.PermissionGroupId == permissionGroupId);
            if (item == null)
            {
                throw new OcudaException($"Unable to find permission for product id {productId} and permission group {permissionGroupId}");
            }
            DbSet.Remove(item);
            await SaveAsync();
        }
    }
}
