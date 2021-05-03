using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IWebslideService
    {
        Task<WebslideItem> CreateItemAsync(WebslideItem webslideItem);
        Task<Webslide> CreateNoSaveAsync(Webslide webslide);
        Task DeleteItemAsync(int webslideItemId);
        Task<Webslide> EditAsync(Webslide webslide);
        Task<WebslideItem> EditItemAsync(WebslideItem webslideItem);
        Task<ICollection<WebslideTemplate>> GetAllTemplatesAsync();
        Task<WebslideItem> GetItemByIdAsync(int id);
        Task<WebslideItemText> GetItemTextByIdsAsync(int webslideItemId, int languageId);
        Task<int?> GetPageHeaderIdForWebslideAsync(int id);
        Task<int> GetPageLayoutIdForWebslideAsync(int id);
        Task<WebslideTemplate> GetTemplateForPageLayoutAsync(int id);
        Task<WebslideTemplate> GetTemplateForWebslideAsync(int id);
        Task<Webslide> GetWebslideDetailsAsync(int id, int languageId);
        Task<WebslideItemText> SetItemTextAsync(WebslideItemText itemText);
        Task UpdateItemSortOrder(int id, bool increase);
    }
}
