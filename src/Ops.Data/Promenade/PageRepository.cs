using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageRepository
        : GenericRepository<PromenadeContext, Page, int>, IPageRepository
    {
        public PageRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<Page>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Stub)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> StubInUseAsync(Page page)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id != page.Id
                    && _.Stub == page.Stub
                    && _.Type == page.Type
                    && _.IsPublished)
                .AnyAsync();
        }
    }
}
