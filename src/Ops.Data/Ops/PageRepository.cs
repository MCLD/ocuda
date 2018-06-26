using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class PageRepository 
        : GenericRepository<Models.Page, int>, IPageRepository
    {
        public PageRepository(OpsContext context, ILogger<PageRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<Page> GetByStubAsync(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Page>>> GetPaginatedListAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId);
            }

            return new DataWithCount<ICollection<Page>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Title)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
