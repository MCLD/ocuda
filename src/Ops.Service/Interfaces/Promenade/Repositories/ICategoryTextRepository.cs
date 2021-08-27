using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ICategoryTextRepository : IGenericRepository<CategoryText>
    {
        Task<ICollection<CategoryText>> GetAllForCategoryAsync(int categoryId);
        Task<CategoryText> GetByCategoryAndLanguageAsync(int categoryId, int languageId);
        Task<ICollection<string>> GetUsedLanguagesForCategoryAsync(int categoryId);
    }
}
