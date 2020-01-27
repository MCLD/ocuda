using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Service.Interfaces.Repositories
{
    public interface ICategoryTextRepository
    {
        Task<CategoryText> GetByIdsAsync(int categoryId, int languageId);
    }
}
