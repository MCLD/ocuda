using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmployeeCardDepartmentRepository
        : GenericRepository<PromenadeContext, EmployeeCardDepartment>,
        IEmployeeCardDepartmentRepository
    {
        public EmployeeCardDepartmentRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmployeeCardDepartmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<EmployeeCardDepartment>> GetSelectableAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsSelectable)
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }
    }
}
