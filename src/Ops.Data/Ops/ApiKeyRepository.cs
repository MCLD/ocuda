using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class ApiKeyRepository : OpsRepository<OpsContext, ApiKey, int>, IApiKeyRepository
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public ApiKeyRepository(Repository<OpsContext> repositoryFacade,
            ILogger<ApiKeyRepository> logger,
            IDateTimeProvider dateTimeProvider)
            : base(repositoryFacade, logger)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);

            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ApiKey> FindByKeyAsync(byte[] apiKey)
        {
            return await DbSet.AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Key == apiKey
                    && (!_.EndDate.HasValue || _.EndDate > _dateTimeProvider.Now));
        }

        public async Task<CollectionWithCount<ApiKey>> PageAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            var data = await query
                .Include(_ => _.CreatedByUser)
                .Include(_ => _.RepresentsUser)
                .OrderBy(_ => _.CreatedAt)
                .AsNoTracking()
                .ApplyPagination(filter)
                .ToListAsync();

            return new CollectionWithCount<ApiKey>
            {
                Count = await query.CountAsync(),
                Data = data
            };
        }
    }
}