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
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class FileLibraryRepository(
        ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<FileLibraryRepository> logger)
        : OpsRepository<OpsContext, FileLibrary, int>(repositoryFacade, logger),
        IFileLibraryRepository
    {
        public async Task AddLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId)
        {
            ArgumentNullException.ThrowIfNull(fileTypeIds);

            foreach (var fileTypeId in fileTypeIds)
            {
                var fileLibraryFileType = new FileLibraryFileType
                {
                    FileLibraryId = libraryId,
                    FileTypeId = fileTypeId,
                };
                await _context.FileLibraryFileTypes.AddAsync(fileLibraryFileType);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is DbUpdateException)
            {
                throw new OcudaException(
                    $"Could not add file type to library: {ex.Message}",
                    ex);
            }
        }

        public override async Task<FileLibrary> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileTypes)
                .ThenInclude(_ => _.FileType)
                .Where(_ => _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<FileLibrary>> GetBySectionAsync(int sectionId,
            bool? isFeatured)
        {
            var libraries = DbSet.AsNoTracking().Where(_ => _.SectionId == sectionId);

            if (isFeatured.HasValue)
            {
                libraries = libraries.Where(_ => _.IsFeatured == isFeatured.Value);
            }

            return await libraries.OrderBy(_ => _.Name).ToListAsync();
        }

        public async Task<FileLibrary> GetBySectionIdSlugAsync(int sectionId, string slug)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId && _.Slug == slug)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId)
        {
            return await _context.FileLibraryFileTypes
                .AsNoTracking()
                .Where(_ => _.FileLibraryId == libraryId)
                .Select(_ => _.FileTypeId)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedListAsync(
                BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            if (filter.SectionId.HasValue)
            {
                query = query.Where(_ => _.SectionId == filter.SectionId.Value);
            }

            return new DataWithCount<ICollection<FileLibrary>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync(),
            };
        }

        public override void Remove(int id)
        {
            var libraryFileTypes = _context.FileLibraryFileTypes.Where(_ => _.FileLibraryId == id);
            _context.FileLibraryFileTypes.RemoveRange(libraryFileTypes);

            base.Remove(id);
        }

        public async Task RemoveLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId)
        {
            ArgumentNullException.ThrowIfNull(fileTypeIds);

            foreach (var fileType in fileTypeIds)
            {
                var fileLibType = _context.FileLibraryFileTypes
                     .Where(_ => _.FileTypeId == fileType && _.FileLibraryId == libraryId)
                     .FirstOrDefault();
                _context.FileLibraryFileTypes.Remove(fileLibType);
            }

            await _context.SaveChangesAsync();
        }
    }
}
