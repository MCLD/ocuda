using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class FileRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<FileRepository> logger)
        : OpsRepository<OpsContext, File, int>(repositoryFacade, logger),
        IFileRepository
    {
        public override async Task<File> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Where(_ => _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<ICollection<File>> GetFileLibraryFilesAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Where(_ => _.FileLibraryId == id)
                .OrderBy(_ => _.Name)
                .ToListAsync();
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

        public async Task<File> GetLatestByLibraryIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Include(_ => _.FileType)
                .Where(_ => _.FileLibraryId == id)
                .OrderByDescending(_ => _.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<File>>> GetPaginatedListAsync(
            FilesFilter filter)
        {
            var baseQuery = DbSet.AsNoTracking();

            if (filter?.FileLibrary?.Id != null)
            {
                baseQuery = baseQuery.Where(_ => _.FileLibraryId == filter.FileLibrary.Id);
            }

            IQueryable<File> filteredQuery = baseQuery.Include(_ => _.FileType)
                .Include(_ => _.CreatedByUser)
                .Include(_ => _.UpdatedByUser);

            filteredQuery = filter.FileLibrary.SortOrder switch
            {
                Models.FileLibrarySort.Date => filteredQuery.OrderBy(_ => _.FileDate),
                Models.FileLibrarySort.Name => filteredQuery.OrderBy(_ => _.Name),
                _ => filteredQuery.OrderByDescending(_ => _.CreatedAt),
            };

            return new DataWithCount<ICollection<File>>
            {
                Count = await baseQuery.CountAsync(),
                Data = await filteredQuery
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}