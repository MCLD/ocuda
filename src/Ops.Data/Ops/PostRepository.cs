﻿using System.Collections.Generic;
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
    public class PostRepository 
        : GenericRepository<Models.Post, int>, IPostRepository
    {
        public PostRepository(OpsContext context, ILogger<PostRepository> logger) 
            : base(context, logger)
        {
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
    }
}
