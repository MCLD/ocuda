using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageLayoutTextRepository : IGenericRepository<PageLayoutText>
    {
        Task<ICollection<PageLayoutText>> GetAllForHeaderAsync(int headerId);
        Task<ICollection<PageLayoutText>> GetAllForLayoutAsync(int layoutId);
        Task<PageLayoutText> GetByPageLayoutAndLanguageAsync(int layoutId, int languageId);
    }
}
