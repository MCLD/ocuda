using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageRepository : IRepository<Page, int>
    {
        Task<ICollection<Page>> GetByHeaderIdAsync(int id);
        Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId);
    }
}
