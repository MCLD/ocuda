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
    public class PermissionGroupPageContentRepository :
        GenericRepository<OpsContext, PermissionGroupPageContent>,
        IPermissionGroupPageContentRepository
    {
        public PermissionGroupPageContentRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupPageContentRepository> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(int pageHeaderId, int permissionGroupId)
        {
            await DbSet.AddAsync(new PermissionGroupPageContent
            {
                PageHeaderId = pageHeaderId,
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

        public async Task<ICollection<PermissionGroupPageContent>>
            GetByPageHeaderId(int pageHeaderId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageHeaderId == pageHeaderId)
                .ToListAsync();
        }

        public async Task<ICollection<PermissionGroupPageContent>>
            GetByPermissionGroupId(int permissionGroupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PermissionGroupId == permissionGroupId)
                .ToListAsync();
        }

        public async Task RemoveSaveAsync(int pageHeaderId, int permissionGroupId)
        {
            var item = await DbSet.SingleOrDefaultAsync(_ => _.PageHeaderId == pageHeaderId
                && _.PermissionGroupId == permissionGroupId);
            if (item == null)
            {
                throw new OcudaException($"Unable to find permission for page id {pageHeaderId} and permission group {permissionGroupId}");
            }
            DbSet.Remove(item);
            await SaveAsync();
        }
    }
}