using System;
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

        Task JobSyncDirectoryAsync(Job job, Func<Job, Task> statusAsync);

        Task<StatusReport> SyncDirectoryAsync(int userId, bool applyChanges);

        Task SyncLocationsAsync(int userId);

        Task UpdateLocationMappingAsync(int userId, int userSyncLocationId, int? mapToLocationId);
    }
}
