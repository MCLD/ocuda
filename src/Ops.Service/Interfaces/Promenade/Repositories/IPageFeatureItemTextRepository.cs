using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IPageFeatureItemTextRepository : IGenericRepository<PageFeatureItemText>
    {
        void DetachEntity(PageFeatureItemText itemText);
        Task<ICollection<PageFeatureItemText>> GetAllForPageFeatureItemAsync(int itemId);
        Task<PageFeatureItemText> GetByPageFeatureItemAndLanguageAsync(int pageFeatureItemId, int languageId);
    }
}
