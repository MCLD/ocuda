using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IBooksByMailCommentRepository : IOpsRepository<BooksByMailComment, int>
    {
        public Task<ICollection<BooksByMailComment>> GetAllAsync(int booksByMailCustomerId);
    }
}