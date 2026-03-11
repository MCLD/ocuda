using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmployeeCardDepartmentRepository :
        GenericRepository<PromenadeContext, EmployeeCardDepartment>, 
        IEmployeeCardDepartmentRepository
    {
        public EmployeeCardDepartmentRepository(Repository<PromenadeContext> repositoryFacade,
            ILogger<EmployeeCardRequest> logger)
            : base(repositoryFacade, logger)
        {
        }

        public async Task<string> GetDepartmentNameAsync(int id)
        {
            return await DbSet.AsNoTracking()
                .Where(_ => _.Id == id)
                .Select(_ => _.Name)
                .SingleOrDefaultAsync();
        }
    }
}
