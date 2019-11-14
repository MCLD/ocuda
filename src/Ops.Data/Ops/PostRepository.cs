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

        public async Task<List<Post>> GetPostsBySectionCategoryIdAsync(int categoryId, int sectionId)
        {
            var posts = await _context.PostCategories
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .Select(_ => _.Post)
                .ToListAsync();
            return posts
                .Where(_ => _.SectionId == sectionId)
                .ToList();
        }

        public Post GetPostByStub(string stub)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == stub)
                .FirstOrDefault();
        }

        public async Task<DataWithCount<ICollection<Post>>> GetSectionCategoryPaginatedListAsync(
            BaseFilter filter, int sectionId, int categoryId)
        {

            return new DataWithCount<ICollection<Post>>
            {
                Count = await _context.PostCategories
                    .AsNoTracking()
                    .Where(_ => _.CategoryId == categoryId
                        && _.Post.SectionId == sectionId)
                    .Select(_ => _.Post)
                    .CountAsync(),
                Data = await _context.PostCategories
                    .AsNoTracking()
                    .Where(_ => _.CategoryId == categoryId
                        && _.Post.SectionId == sectionId)
                    .Select(_=>_.Post)
                    .OrderByDescending(_ => _.PublishedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedListAsync(
            BaseFilter filter, int sectionId)
        {
            return new DataWithCount<ICollection<Post>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => _.SectionId == sectionId)
                    .OrderByDescending(_=>_.PublishedAt)
                    .CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Title)
                    .Where(_ => _.SectionId == sectionId)
                    .OrderByDescending(_ => _.PublishedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<List<Post>> GetTopSectionPosts(int sectionId, int count)
        {
            return await _context.PostCategories
                .AsNoTracking()
                .Where(_ => _.Post.SectionId == sectionId)
                .Select(_ => _.Post)
                .OrderByDescending(_ => _.PublishedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<PostCategory>> GetPostCategory(int id)
        {
                return await _context.PostCategories
                    .AsNoTracking()
                    .Where(_ => _.PostId == id)
                    .ToListAsync();
        }

        public async Task AddPostCategory(List<int> categories, int postId)
        {
            foreach (var category in categories)
            {
                var postCat = new PostCategory()
                {
                    PostId = postId,
                    Post = _context.Posts.Find(postId),
                    CategoryId = category,
                    Category = _context.Categories.Find(postId)
                };
                _context.PostCategories.Add(postCat);
            }
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostCategory(List<int> categories, int postId)
        {
            foreach (var category in categories)
            {
                var postCat = _context.PostCategories
                    .Where(_ => _.CategoryId == category && _.PostId == postId)
                    .FirstOrDefault();
                _context.PostCategories.Remove(postCat);
            }
            await _context.SaveChangesAsync();
        }
    }
}
