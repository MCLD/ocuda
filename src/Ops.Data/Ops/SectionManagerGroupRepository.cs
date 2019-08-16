using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Data;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionManagerGroupRepository
        : GenericRepository<OpsContext, SectionManagerGroup, int>, ISectionManagerGroupRepository
    {
        public SectionManagerGroupRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SectionManagerGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
