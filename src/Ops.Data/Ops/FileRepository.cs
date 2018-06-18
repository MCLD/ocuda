using Microsoft.Extensions.Logging;
using Ocuda.Utility.Data;

namespace Ocuda.Ops.Data.Ops
{
    public class FileRepository : GenericRepository<Models.File, int>
    {
        public FileRepository(OpsContext context, ILogger<FileRepository> logger)
            : base(context, logger)
        {
        }
    }
}
