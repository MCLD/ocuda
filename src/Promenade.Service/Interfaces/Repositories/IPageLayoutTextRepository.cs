using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface IPageLayoutTextRepository : IGenericRepository<PageLayoutText>
    {
        Task<PageLayoutText> GetByIdsAsync(int layoutId, int languageId);
    }
}
