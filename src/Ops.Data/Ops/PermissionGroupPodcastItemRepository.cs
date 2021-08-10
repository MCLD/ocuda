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
    public class PermissionGroupPodcastItemRepository
        : GenericRepository<OpsContext, PermissionGroupPodcastItem>,
        IPermissionGroupPodcastItemRepository
    {
        public PermissionGroupPodcastItemRepository(Repository<OpsContext> repositoryFacade,
            ILogger<PermissionGroupPodcastItemRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(int podcastId, int permissionGroupId)
        {
            await DbSet.AddAsync(new PermissionGroupPodcastItem
            {
                PodcastId = podcastId,
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

        public async Task RemoveSaveAsync(int podcastId, int permissionGroupId)
        {
            var item = await DbSet.SingleOrDefaultAsync(_ => _.PodcastId == podcastId
                && _.PermissionGroupId == permissionGroupId);
            if (item == null)
            {
                throw new OcudaException($"Unable to find permission for page id {podcastId} and permission group {permissionGroupId}");
            }
            DbSet.Remove(item);
            await SaveAsync();
        }
    }
}