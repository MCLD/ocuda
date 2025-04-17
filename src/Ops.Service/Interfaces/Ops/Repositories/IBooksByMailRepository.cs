using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IBooksByMailRepository
    {
        Task<BooksByMailCustomer> GetByIdAsync(int id);

        Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId);

        Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer);

        Task UpdateAsync(BooksByMailCustomer customer);

        Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment);
    }
}