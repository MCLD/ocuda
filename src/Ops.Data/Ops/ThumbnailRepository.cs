using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class ThumbnailRepository : GenericRepository<Thumbnail, int>, IThumbnailRepository
    {
        public ThumbnailRepository(OpsContext context, ILogger<ThumbnailRepository> logger)
            : base (context, logger)
        {

        }

        public void RemoveByFileId(int fileId)
        {
            var elements = DbSet.Where(_ => _.FileId == fileId);
            DbSet.RemoveRange(elements);
        }
    }
}
