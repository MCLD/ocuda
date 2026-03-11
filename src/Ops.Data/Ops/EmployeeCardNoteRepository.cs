using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class EmployeeCardNoteRepository : OpsRepository<OpsContext, EmployeeCardNote, int>,
        IEmployeeCardNoteRepository
    {
        public EmployeeCardNoteRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<EmployeeCardNoteRepository> logger) : base(repositoryFacade, logger)
        {
        }
    }
}
