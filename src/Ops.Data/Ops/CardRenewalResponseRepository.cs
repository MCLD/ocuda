using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class CardRenewalResponseRepository 
        : OpsRepository<OpsContext, CardRenewalResponse, int>, 
        ICardRenewalResponseRepository
    {
        public CardRenewalResponseRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<CardRenewalResponseRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public override async Task<CardRenewalResponse> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<CardRenewalResponse> GetBySortOrderAsync(int sortOrder)
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

        public async Task<IEnumerable<CardRenewalResponse>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.EmailSetup)
                .Where(_ => !_.IsDeleted)
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<IEnumerable<CardRenewalResponse>> GetAvailableAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.EmailSetupId.HasValue)
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<List<CardRenewalResponse>> GetSubsequentAsync(int sortOrder)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !_.IsDeleted && _.SortOrder > sortOrder)
                .ToListAsync();
        }
    }
}
