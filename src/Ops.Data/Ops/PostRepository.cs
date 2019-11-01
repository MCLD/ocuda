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
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId && _.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Post>>> GetSectionCategoryPaginatedListAsync(
            BaseFilter filter, int sectionId, int categoryId)
        {
            var sectionCats = _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId && _.CategoryId == categoryId)
                .ToList();
            return new DataWithCount<ICollection<Post>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => sectionCats.Select(__ => __.SectionId).Contains(_.SectionId)
                        && sectionCats.Select(__ => __.CategoryId).Contains(_.CategoryId))
                    .OrderByDescending(_ => _.PublishedAt)
                    .CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Title)
                    .Where(_ => sectionCats.Select(__ => __.SectionId).Contains(_.SectionId)
                        && sectionCats.Select(__ => __.CategoryId).Contains(_.CategoryId))
                    .OrderByDescending(_ => _.PublishedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedListAsync(
            BaseFilter filter, int sectionId)
        {
            var sectionCats = _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToList();
            return new DataWithCount<ICollection<Post>>
            {
                Count = await DbSet
                    .AsNoTracking()
                    .Where(_ => sectionCats.Select(__ => __.SectionId).Contains(_.SectionId))
                    .OrderByDescending(_=>_.PublishedAt)
                    .CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Title)
                    .Where(_ => sectionCats.Select(__ => __.SectionId).Contains(_.SectionId))
                    .OrderByDescending(_ => _.PublishedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<List<Post>> GetTopSectionPosts(int sectionId, int count)
        {
            var sectionCats = await _context.SectionCategories
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToListAsync();
            return await DbSet.AsNoTracking()
                .OrderBy(_ => _.Title)
                .Where(_ => sectionCats.Select(__ => __.SectionId).Contains(_.SectionId)
                    && sectionCats.Select(__ => __.CategoryId).Contains(_.CategoryId))
                .OrderByDescending(_ => _.PublishedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}
