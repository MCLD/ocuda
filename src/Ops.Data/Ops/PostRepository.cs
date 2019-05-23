using System;
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
    public class PostRepository
        : GenericRepository<Post, int>, IPostRepository
    {
        public PostRepository(OpsContext context, ILogger<PostRepository> logger)
            : base(context, logger)
        {
        }

        public override Task<Post> FindAsync(int id)
        {
            return DbSet
                .AsNoTracking()
                .Include(_ => _.PostCategory)
                .Where(_ => _.Id == id)
                .SingleAsync();
        }

        public async Task<Post> GetByStubAndCategoryIdAsync(string stub, int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub
                         && _.PostCategoryId == categoryId)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.PostCategoryId.HasValue)
            {
                query = query.Where(_ => _.PostCategoryId == filter.PostCategoryId.Value);
            }
            else if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.PostCategory.SectionId == filter.SectionId.Value);
            }

            return new DataWithCount<ICollection<Post>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .Include(_ => _.PostCategory)
                    .OrderByDescending(_ => _.IsPinned)
                    .ThenByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<bool> StubInUseAsync(Post post)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == post.Stub
                         && _.PostCategoryId == post.PostCategoryId
                         && _.Id != post.Id
                         && _.IsDraft == false)
                .AnyAsync();
        }
    }
}
