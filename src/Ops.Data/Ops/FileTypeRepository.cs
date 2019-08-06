using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Data;

namespace Ocuda.Ops.Data.Ops
{
    public class FileTypeRepository
        : GenericRepository<OpsContext, FileType, int>, IFileTypeRepository
    {
        public FileTypeRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<FileTypeRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<FileType>> GetAllExtensionsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Extension)
                .ToListAsync();
        }

        public async Task<FileType> GetByExtensionAsync(string extension)
        {
            return await DbSet
                    .AsNoTracking()
                    .Where(_ => _.Extension == extension)
                    .SingleOrDefaultAsync();
        }
    }
}
