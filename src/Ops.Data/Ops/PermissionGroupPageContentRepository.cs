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
    public class PermissionGroupPageContentRepository :
        GenericRepository<OpsContext, PermissionGroupPageContent>,
        IPermissionGroupPageContentRepository
    {
        public PermissionGroupPageContentRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupPageContentRepository> logger)
            : base(repositoryFacade, logger)
        {
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
    }
}
