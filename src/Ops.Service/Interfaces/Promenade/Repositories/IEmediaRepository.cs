using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaRepository : IRepository<Emedia, int>
    {
        Task<ICollection<Emedia>> GetAllAsync();

        Emedia GetByStub(string emediaStub);

        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListAsync(
            BaseFilter filter);
    }
}
