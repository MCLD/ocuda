using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class FileTypeRepository : GenericRepository<FileType, int>, IFileTypeRepository
    {
        public FileTypeRepository(OpsContext context, ILogger<FileTypeRepository> logger)
            : base(context, logger)
        {
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
    }
}
