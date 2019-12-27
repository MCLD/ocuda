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
    public class PostRepository : GenericRepository<OpsContext, Post, int>, IPostRepository
    {
        public PostRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<PostRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Post>> GetPostsBySectionCategoryIdAsync(int categoryId, int sectionId)
        {
            return await _context.PostCategories
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId && _.Post.SectionId == sectionId)
                .Select(_ => _.Post)
                .ToListAsync();
        }

        public async Task<Post> GetSectionPostByStubAsync(string stub, int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub && _.SectionId == sectionId)
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListBySectionAsync(
            BaseFilter filter, int sectionId, int? categoryId = null)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId);

            if (categoryId.HasValue)
            {
                query = query
                    .Join(_context.PostCategories,
                        post => post.Id,
                        postCategory => postCategory.PostId,
                        (post, postCategory) => new { post, postCategory })
                    .Where(_ => _.postCategory.CategoryId == categoryId.Value)
                    .Select(_ => _.post);
            }

            return new DataWithCount<ICollection<Post>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.PublishedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<List<Post>> GetTopSectionPostsAsync(int sectionId, int count)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .OrderByDescending(_ => _.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<PostCategory>> GetPostCategoriesAsync(int id)
        {
            return await _context.PostCategories
                .AsNoTracking()
                .Where(_ => _.PostId == id)
                .ToListAsync();
        }

        public async Task AddPostCategoriesAsync(List<int> categories, int postId)
        {
            foreach (var category in categories)
            {
                var postCategory = new PostCategory
                {
                    PostId = postId,
                    CategoryId = category
                };
                _context.PostCategories.Add(postCategory);
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeletePostCategoriesAsync(List<int> categories, int postId)
        {
            foreach (var category in categories)
            {
                var postCategory = _context.PostCategories
                    .Where(_ => _.CategoryId == category && _.PostId == postId)
                    .FirstOrDefault();
                _context.PostCategories.Remove(postCategory);
            }

            await _context.SaveChangesAsync();
        }
    }
}
