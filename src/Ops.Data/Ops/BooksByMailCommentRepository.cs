using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class BooksByMailCommentRepository :
        OpsRepository<OpsContext, BooksByMailComment, int>,
        IBooksByMailCommentRepository
    {
        public BooksByMailCommentRepository(Repository<OpsContext> repositoryFacade,
            ILogger<BooksByMailCommentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<BooksByMailComment>> GetAllAsync(int booksByMailCustomerId)
        {
            return await DbSet
                .Where(_ => _.BooksByMailCustomerId == booksByMailCustomerId)
                .OrderByDescending(_ => _.CreatedAt)
                .ToListAsync();
        }
    }
}