using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class UserSyncHistoryRepository
        : OpsRepository<OpsContext, UserSyncHistory, int>, IUserSyncHistoryRepository
    {
        public UserSyncHistoryRepository(Repository<OpsContext> repositoryFacade,
            ILogger<UserSyncHistoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<CollectionWithCount<UserSyncHistory>> GetPaginatedAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();
            return new CollectionWithCount<UserSyncHistory>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .Include(_ => _.CreatedByUser)
                    .Select(_ => new UserSyncHistory
                    {
                        AddedUsers = _.AddedUsers,
                        CreatedAt = _.CreatedAt,
                        CreatedBy = _.CreatedBy,
                        CreatedByUser = _.CreatedByUser,
                        DeletedUsers = _.DeletedUsers,
                        Id = _.Id,
                        TotalRecords = _.TotalRecords,
                        UndeletedUsers = _.UndeletedUsers,
                        UpdatedUsers = _.UpdatedUsers
                    })
                    .ToListAsync()
            };
        }
    }
}