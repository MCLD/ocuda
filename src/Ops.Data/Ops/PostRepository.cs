using System;
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
    public class PostRepository 
        : GenericRepository<Models.Post, int>, IPostRepository
    {
        public PostRepository(OpsContext context, ILogger<PostRepository> logger) 
            : base(context, logger)
        {
        }

        public async Task<Post> GetByStubAsync(string stub)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub)
                .FirstOrDefaultAsync();
        }

        public async Task<Post> GetByStubAndSectionIdAsync(string stub, int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub
                         && _.SectionId == sectionId)
                .FirstOrDefaultAsync();
        }

        public async Task<Post> GetByTitleAndSectionIdAsync(string title, int sectionId)
        {
            return await DbSet
                    .AsNoTracking()
                    .Where(_ => _.Title == title
                             && _.SectionId == sectionId)
                    .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId);
            }

            return new DataWithCount<ICollection<Post>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.IsPinned)
                    .ThenByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> StubInUseAsync(string stub, int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub
                         && _.SectionId == sectionId 
                         && _.IsDraft == false)
                .AnyAsync();
        }
    }
}
