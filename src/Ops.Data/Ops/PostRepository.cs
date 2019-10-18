using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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
    public class PostRepository : GenericRepository<OpsContext, Post, int>, IPostRepository
    {
        public PostRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<PostRepository> logger) : base(repositoryFacade, logger)
        {
        }
        public async Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PostCategoryId == categoryId)
                .ToListAsync();
        }
        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(
            BaseFilter filter, int categoryId)
        {
            return new DataWithCount<ICollection<Post>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => _.PostCategoryId == categoryId)
                    .CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Title)
                    .Where(_ => _.PostCategoryId == categoryId)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
        public async Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedListAsync(
            BaseFilter filter, List<PostCategory> categories)
        {
            return new DataWithCount<ICollection<Post>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => categories.Select(__ => __.Id).Contains(_.PostCategoryId))
                    .OrderByDescending(_=>_.PublishedAt)
                    .CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Title)
                    .Where(_ => categories.Select(__ => __.Id).Contains(_.PostCategoryId))
                    .OrderByDescending(_ => _.PublishedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
