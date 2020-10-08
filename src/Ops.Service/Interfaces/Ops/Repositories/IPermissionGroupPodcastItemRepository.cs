using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPermissionGroupPodcastItemRepository
        : IGenericRepository<PermissionGroupPodcastItem>
    {
        public Task<ICollection<PermissionGroupPodcastItem>>
            GetByPermissionGroupId(int permissionGroupId);
        public Task<ICollection<PermissionGroupPodcastItem>> GetByPodcastId(int podcastId);

        public Task<bool> AnyPermissionGroupIdAsync(IEnumerable<int> permissionGroupIds);
    }
}
