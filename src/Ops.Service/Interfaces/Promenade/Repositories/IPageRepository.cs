using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageRepository : IGenericRepository<Page>
    {
        Task<ICollection<Page>> GetByHeaderIdAsync(int id);
        Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId);
    }
}
