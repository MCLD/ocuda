using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class RenewCardResponseRepository
        : OpsRepository<OpsContext, RenewCardResponse, int>,
        IRenewCardResponseRepository
    {
        public RenewCardResponseRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<RenewCardResponseRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public override async Task<RenewCardResponse> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<RenewCardResponse> GetBySortOrderAsync(int sortOrder)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.SortOrder == sortOrder)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetMaxSortOrderAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted)
                .MaxAsync(_ => (int?)_.SortOrder);
        }

        public async Task<IEnumerable<RenewCardResponse>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.EmailSetup)
                .Where(_ => !_.IsDeleted)
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<RenewCardResponse>> GetAvailableAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.EmailSetupId.HasValue)
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<List<RenewCardResponse>> GetSubsequentAsync(int sortOrder)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.SortOrder > sortOrder)
                .ToListAsync();
        }
    }
}
