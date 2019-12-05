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
    public class FileLibraryRepository
        : GenericRepository<OpsContext, FileLibrary, int>, IFileLibraryRepository
    {
        public FileLibraryRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<FileLibraryRepository> logger) : base(repositoryFacade, logger)
        {
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

        public override void Remove(int id)
        {
            var libraryFileTypes = _context.FileLibraryFileTypes.Where(_ => _.FileLibraryId == id);
            _context.FileLibraryFileTypes.RemoveRange(libraryFileTypes);

            base.Remove(id);
        }

        public async Task RemoveLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId)
        {
            foreach (var fileType in fileTypeIds)
            {
                var fileLibType = _context.FileLibraryFileTypes
                     .Where(_ => _.FileTypeId == fileType && _.FileLibraryId == libraryId)
                     .FirstOrDefault();
                _context.FileLibraryFileTypes.Remove(fileLibType);
            }
            await _context.SaveChangesAsync();
        }

        public async Task AddLibraryFileTypesAsync(List<int> fileTypeIds, int libraryId)
        {
            foreach (var fileType in fileTypeIds)
            {
                var fileLibType = new FileLibraryFileType
                {
                    FileLibraryId = libraryId,
                    FileTypeId = fileType
                };
                await _context.FileLibraryFileTypes.AddAsync(fileLibType);
            }
            await _context.SaveChangesAsync();
        }

        public async Task<DataWithCount<ICollection<FileLibrary>>> GetPaginatedListAsync(
                BlogFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<FileLibrary>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderByDescending(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<ICollection<int>> GetLibraryFileTypeIdsAsync(int libraryId)
        {
            return await _context.FileLibraryFileTypes
                .AsNoTracking()
                .Where(_ => _.FileLibraryId == libraryId)
                .Select(_ => _.FileTypeId)
                .ToListAsync();
        }

        public async Task<List<FileLibrary>> GetFileLibrariesBySectionIdAsync(int sectionId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SectionId == sectionId)
                .ToListAsync();
        }
    }
}
