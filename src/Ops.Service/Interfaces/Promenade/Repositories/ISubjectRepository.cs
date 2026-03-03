using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        Task<Subject> FindAsync(int id);

        Task<ICollection<Subject>> GetAllAsync();

        Task<DataWithCount<ICollection<Subject>>> GetPaginatedListAsync(BaseFilter filter);

        Task<string> GetUnusedSlugAsync(string slug);
    }
}