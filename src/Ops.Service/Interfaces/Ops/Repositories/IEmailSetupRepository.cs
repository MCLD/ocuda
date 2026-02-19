using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IEmailSetupRepository : IGenericRepository<EmailSetup>
    {
        Task<IEnumerable<EmailSetup>> GetAllAsync();
    }
}
