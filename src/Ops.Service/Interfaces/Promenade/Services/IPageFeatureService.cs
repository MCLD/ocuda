using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IPageFeatureService
    {
        Task<PageFeatureItem> CreateItemAsync(PageFeatureItem featureItem);
        Task<PageFeature> CreateNoSaveAsync(PageFeature feature);
        Task DeleteItemAsync(int featureItemId);
        Task<PageFeature> EditAsync(PageFeature feature);
        Task<PageFeatureItem> EditItemAsync(PageFeatureItem featureItem);
        Task<ICollection<PageFeatureTemplate>> GetAllTemplatesAsync();
        Task<PageFeatureItem> GetItemByIdAsync(int id);
        Task<PageFeatureItemText> GetItemTextByIdsAsync(int featureItemId, int languageId);
        Task<PageFeature> GetPageFeatureDetailsAsync(int id, int languageId);
        Task<int?> GetPageHeaderIdForPageFeatureAsync(int id);
        Task<int> GetPageLayoutIdForPageFeatureAsync(int id);
        Task<PageFeatureTemplate> GetTemplateForPageFeatureAsync(int id);
        Task<PageFeatureItemText> SetItemTextAsync(PageFeatureItemText itemText);
        Task UpdateItemSortOrder(int id, bool increase);
    }
}
