using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class PostCategoryRepository
        : GenericRepository<PostCategory, int>, IPostCategoryRepository
    {
        public PostCategoryRepository(OpsContext context, ILogger<PostCategoryRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<IEnumerable<PostCategory>> GetBySectionIdAsync(int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedListAsync(
            BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId.Value);
            }

            return new DataWithCount<ICollection<PostCategory>>
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
