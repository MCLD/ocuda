using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class FileTypeRepository : GenericRepository<FileType, int>, IFileTypeRepository
    {
        public FileTypeRepository(OpsContext context, ILogger<FileTypeRepository> logger)
            : base(context, logger)
        {
        }

        public async Task<ICollection<FileType>> GetAllExtensionsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsDefault == false || !string.IsNullOrWhiteSpace(_.Extension))
                .OrderBy(_ => _.Extension)
                .ToListAsync();
        }

        public async Task<FileType> GetByExtensionAsync(string extension)
        {
            var query = DbSet
                    .AsNoTracking()
                    .Where(_ => _.Extension == extension);

            if (query.Count() == 0)
            {
                query = DbSet
                    .AsNoTracking()
                    .Where(_ => _.IsDefault == true);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<int> GetIdByExtensionAsync(string extension)
        {
            var query = DbSet
                .AsNoTracking()
                .Where(_ => _.Extension == extension)
                .Select(_ => _.Id);

            if (query.Count() == 0)
            {
                query = DbSet
                    .AsNoTracking()
                    .Where(_ => _.IsDefault == true)
                    .Select(_ => _.Id);
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
