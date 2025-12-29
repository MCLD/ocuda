using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class BooksByMailCustomerRepository :
        OpsRepository<OpsContext, BooksByMailCustomer, int>,
        IBooksByMailCustomerRepository
    {
        public BooksByMailCustomerRepository(Repository<OpsContext> repositoryFacade,
            ILogger<BooksByMailCustomerRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<BooksByMailCustomer> GetCustomerAsync(int customerLookupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.ExternalCustomerId == customerLookupId)
                .SingleOrDefaultAsync();
        }
    }
}