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
    public class FileRepository 
        : GenericRepository<File, int>, IFileRepository
    {
        public FileRepository(OpsContext context, ILogger<FileRepository> logger)
            : base(context, logger)
        {
        }

        public override async Task<File> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Include(_ => _.Thumbnails)
                .Where(_ => _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<File> GetLatestByLibraryIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Include(_ => _.Thumbnails)
                .Where(_ => _.FileLibraryId == id)
                .OrderByDescending(_ => _.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            BlogFilter filter, bool isGallery)
        {
            var query = DbSet.AsNoTracking();

            if (filter.FileLibraryId.HasValue)
            {
                query = query.Where(_ => _.FileLibraryId == filter.FileLibraryId.Value);
            }
            else if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.FileLibrary.SectionId == filter.SectionId.Value);
            }

            if (isGallery)
            {
                query = query.Where(_ => _.Thumbnails.Count > 0);
            }

            return new DataWithCount<ICollection<File>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .Include(_ => _.FileType)
                    .Include(_ => _.Thumbnails)
                    .OrderByDescending(_ => _.CreatedAt)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<ICollection<int>> GetFileTypeIdsInUseByLibraryAsync(int libraryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.FileLibraryId == libraryId)
                .Select(_ => _.FileTypeId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<IEnumerable<File>> GetByPageIdAsync(int pageId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Where(_ => _.PageId == pageId)
                .ToListAsync();
        }

        public async Task<IEnumerable<File>> GetByPostIdAsync(int postId)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Where(_ => _.PostId == postId)
                .ToListAsync();
        }
    }
}
