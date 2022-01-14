using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface INavigationTextRepository : IGenericRepository<NavigationText>
    {
        Task<NavigationText> GetByNavigationAndLanguageAsync(int navigationId, int languageId);
        Task<ICollection<NavigationText>> GetByNavigationIdsAsync(List<int> navigationIds);
        Task<List<string>> GetUsedLanguageNamesByNavigationId(int navigationId);
    }
}
