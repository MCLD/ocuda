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
    public class PermissionGroupPodcastItemRepository
        : GenericRepository<OpsContext, PermissionGroupPodcastItem>,
        IPermissionGroupPodcastItemRepository
    {
        public PermissionGroupPodcastItemRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupPodcastItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => permissionGroupIds.Contains(_.PermissionGroupId))
                .AnyAsync();
        }

        public async Task<ICollection<PermissionGroupPodcastItem>>
            GetByPermissionGroupId(int permissionGroupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PermissionGroupId == permissionGroupId)
                .ToListAsync();
        }

        public async Task<ICollection<PermissionGroupPodcastItem>>
            GetByPodcastId(int podcastId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PodcastId == podcastId)
                .ToListAsync();
        }
    }
}
