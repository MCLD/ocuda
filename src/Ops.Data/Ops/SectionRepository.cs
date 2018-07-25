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
                .Where(_ => string.IsNullOrEmpty(_.Path) && _.IsDeleted == false)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Section>> GetNavigationSectionsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => !string.IsNullOrWhiteSpace(_.Icon) && _.IsDeleted == false)
                .OrderBy(_ => _.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> IsValidPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == path && _.IsDeleted == false)
                .AnyAsync();
        }

        public async Task<Section> GetByPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == path && _.IsDeleted == false)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Section>>> GetPaginatedListAsync
            (BaseFilter filter) {
            var query = DbSet.AsNoTracking().Where(_ => _.IsDeleted == false);

            return new DataWithCount<ICollection<Section>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.SortOrder)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public override async Task<Section> FindAsync(int id)
        {
            var section = await DbSet
                .Where(_ => _.Id == id && _.IsDeleted == false)
                .FirstOrDefaultAsync();
            _context.Entry(section).State = EntityState.Detached;
            return section;
        }
    }
}
