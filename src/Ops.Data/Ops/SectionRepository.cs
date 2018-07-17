using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionRepository 
        : GenericRepository<Models.Section, int>, ISectionRepository
    {
        public SectionRepository(OpsContext context, ILogger<SectionRepository> logger) 
            : base(context, logger)
        {
        }

        public Task<Section> GetDefaultSectionAsync()
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => string.IsNullOrEmpty(_.Path))
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Section>> GetNavigationSectionsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !string.IsNullOrWhiteSpace(_.Icon))
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> IsValidPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == path)
                .AnyAsync();
        }

        public async Task<Section> GetByPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == path)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Section>>> GetPaginatedListAsync
            (BaseFilter filter) {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<Section>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.SortOrder)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
