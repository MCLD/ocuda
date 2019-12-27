using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IExternalResourceRepository : IGenericRepository<ExternalResource, int>
    {
        Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type);
    }
}
