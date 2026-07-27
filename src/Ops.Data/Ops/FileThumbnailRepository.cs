using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class FileThumbnailRepository(
        ServiceFacade.Repository<OpsContext> repositoryFacade,
        ILogger<FileThumbnailRepository> logger)
        : OpsRepository<OpsContext, FileThumbnail, int>(repositoryFacade, logger),
        IFileThumbnailRepository
    {
        public async Task<ICollection<FileThumbnail>> GetByFileIdsAsync(IEnumerable<int> fileIds)
        {
            return await DbSet.AsNoTracking().Where(_ => fileIds.Contains(_.FileId)).ToListAsync();
        }
    }
}
