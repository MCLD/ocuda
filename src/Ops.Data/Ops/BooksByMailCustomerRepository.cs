using System.Linq;
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

        public async Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId)
        {
            var booksByMailCustomer = await _context.BooksByMailCustomers
                .AsNoTracking()
                .Include(_ => _.Comments)
                .Where(_ => _.CustomerLookupID == customerLookupId)
                .FirstOrDefaultAsync();

            if (booksByMailCustomer?.Comments != null)
            {
                booksByMailCustomer.Comments = booksByMailCustomer.Comments.OrderByDescending(_ => _.CreatedAt).ToList();
            }

            return booksByMailCustomer;
        }

        public async Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment)
        {
            await _context.BooksByMailComments.AddAsync(comment);
            await _context.SaveChangesAsync();

            return comment;
        }
    }
}