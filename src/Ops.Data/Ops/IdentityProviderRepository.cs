using System;
using System.Collections.Generic;
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
    public class IdentityProviderRepository
            : OpsRepository<OpsContext, IdentityProvider, int>, IIdentityProviderRepository
    {
        public IdentityProviderRepository(Repository<OpsContext> repositoryFacade,
            ILogger<IdentityProviderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<IdentityProvider> GetActiveAsync(string slug)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.IsActive && _.Slug == slug);
        }

        public async Task<IEnumerable<IdentityProvider>> GetAllActiveAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<IdentityProvider>>> PageAsync(BaseFilter filter)
        {
            ArgumentNullException.ThrowIfNull(filter);

            var query = DbSet
                .Include(_ => _.CreatedByUser)
                .AsNoTracking();

            return new DataWithCount<ICollection<IdentityProvider>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}