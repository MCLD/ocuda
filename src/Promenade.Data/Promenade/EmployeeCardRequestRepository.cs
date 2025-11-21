using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class EmployeeCardRequestRepository
        : GenericRepository<PromenadeContext, EmployeeCardRequest>,
            IEmployeeCardRequestRepository
    {
        public EmployeeCardRequestRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmployeeCardRequestRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task AddSaveAsync(EmployeeCardRequest cardRequest)
        {
            await DbSet.AddAsync(cardRequest);
            await _context.SaveChangesAsync();
        }
    }
}
