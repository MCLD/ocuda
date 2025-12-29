using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IBooksByMailService
    {
        Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer);

        Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment);

        Task<BooksByMailCustomer> GetAsync(int booksByMailCustomerId);

        Task<BooksByMailCustomer> UpdateCustomerAsync(BooksByMailCustomer customer);
    }
}