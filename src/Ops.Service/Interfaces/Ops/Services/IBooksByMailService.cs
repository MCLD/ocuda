using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IBooksByMailService
    {
        Task<BooksByMailCustomer> GetByIdAsync(int id);

        Task<BooksByMailCustomer> GetByPatronIdAsync(int patronId);

        Task<BooksByMailCustomer> AddAsync(BooksByMailCustomer customer);

        Task UpdateAsync(BooksByMailCustomer customer);

        Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment);
    }
}