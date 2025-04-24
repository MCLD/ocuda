using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IBooksByMailService
    {
        Task<BooksByMailCustomer> FindAsync(int id);

        Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId);

        Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer);

        Task<BooksByMailCustomer> UpdateCustomer(BooksByMailCustomer customer);

        Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment);
    }
}