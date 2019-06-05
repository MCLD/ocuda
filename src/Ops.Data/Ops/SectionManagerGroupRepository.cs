using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionManagerGroupRepository
        : GenericRepository<SectionManagerGroup, int>, ISectionManagerGroupRepository
    {
        public SectionManagerGroupRepository(OpsContext context, 
            ILogger<SectionManagerGroupRepository> logger)
            : base(context, logger)
        {
        }
    }
}
