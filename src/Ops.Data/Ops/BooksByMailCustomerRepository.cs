using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class BooksByMailCustomerRepository : OpsRepository<OpsContext, BooksByMailCustomer, int>, IBooksByMailRepository
    {
        public BooksByMailCustomerRepository(Repository<OpsContext> repositoryFacade,
            ILogger<BooksByMailCustomerRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<BooksByMailCustomer> GetByIdAsync(int id)
        {
            return await _context.BooksByMailCustomers
                .AsNoTracking()
                .Where(_ => _.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId)
        {
            var booksbymailcustomer = await _context.BooksByMailCustomers
                .AsNoTracking()
                .Include(_ => _.Comments)
                .Where(_ => _.CustomerLookupID == customerLookupId)
                .FirstOrDefaultAsync();

            if (booksbymailcustomer?.Comments != null)
            {
                booksbymailcustomer.Comments = booksbymailcustomer.Comments.OrderByDescending(_ => _.CreatedAt).ToList();
            }

            return booksbymailcustomer;
        }

        public async Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            await _context.BooksByMailComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }
    }
}
