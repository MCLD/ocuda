using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IBooksByMailRepository : IOpsRepository<BooksByMailCustomer, int>
    {
        Task<BooksByMailCustomer> GetByIdAsync(int id);

        Task<BooksByMailCustomer> GetByCustomerLookupIdAsync(int customerLookupId);

        Task<BooksByMailComment> AddCommentAsync(BooksByMailComment comment);
    }
}
