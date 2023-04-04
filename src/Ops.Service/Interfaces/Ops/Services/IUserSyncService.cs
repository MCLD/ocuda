using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IUserSyncService
    {
        Task<StatusReport> CheckSyncLocationsAsync();

        Task<StatusReport> GetImportDetailAsync(int id);

        Task<ICollection<UserSyncLocation>> GetLocationsAsync();

        Task<CollectionWithCount<UserSyncHistory>> GetPaginatedHeadersAsync(BaseFilter filter);

        Task<StatusReport> SyncDirectoryAsync(bool applyChanges);

        Task SyncLocationsAsync();

        Task UpdateLocationMappingAsync(int userSyncLocationId, int? mapToLocationId);
    }
}