using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Ops;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionRepository 
        : GenericRepository<Models.Section, int>, ISectionRepository
    {
        public SectionRepository(OpsContext context, ILogger<SectionRepository> logger) 
            : base(context, logger)
        {
        }

        #region Initial setup methods
        public Task<Models.Section> GetDefaultSectionAsync()
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => string.IsNullOrEmpty(_.Path))
                .FirstOrDefaultAsync();
        }
        #endregion Initial setup methods
    }
}
