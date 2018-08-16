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
    public class FileRepository 
        : GenericRepository<Models.File, int>, IFileRepository
    {
        public FileRepository(OpsContext context, ILogger<FileRepository> logger)
            : base(context, logger)
        {
        }

        public override async Task<File> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.Thumbnails)
                .Where(_ => _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            BlogFilter filter, bool isGallery)
        {
            var query = DbSet.AsNoTracking();

            query = query.Include(_ => _.Category);
            query = query.Where(_ => _.Category.IsAttachment == false);

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(_ => _.CategoryId == filter.CategoryId);
            }
            else if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId);
            }

            query = query.Include(_ => _.FileType);
            query = query.Include(_ => _.Thumbnails);

            if (isGallery)
            {
                query = query.Where(_ => _.Thumbnails.Count > 0);
            }

            return new DataWithCount<ICollection<File>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<IEnumerable<int>> GetFileTypeIdsInUseByCategoryId(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .Select(_ => _.FileTypeId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<File>> GetByPageIdAsync(int pageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageId == pageId)
                .ToListAsync();
        }

        public async Task<IEnumerable<File>> GetByPostIdAsync(int postId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PostId == postId)
                .ToListAsync();
        }
    }
}
