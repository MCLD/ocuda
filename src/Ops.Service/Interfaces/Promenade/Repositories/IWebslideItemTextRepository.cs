using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IWebslideItemTextRepository : IGenericRepository<WebslideItemText>
    {
        void DetachEntity(WebslideItemText itemText);
        Task<ICollection<WebslideItemText>> GetAllForWebslideItemAsync(int itemId);
        Task<WebslideItemText> GetByWebslideItemAndLanguageAsync(int webslideItemId, int languageId);
    }
}
