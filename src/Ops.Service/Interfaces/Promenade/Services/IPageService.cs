using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IPageService
    {
        Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId);
        Task<Page> CreateAsync(Page page);
        Task<Page> EditAsync(Page page);
        Task DeleteAsync(Page page);
        Task<DataWithCount<ICollection<PageHeader>>> GetPaginatedHeaderListAsync(PageFilter filter);
        Task<PageHeader> GetHeaderByIdAsync(int id);
        Task<ICollection<string>> GetHeaderLanguagesByIdAsync(int id);
        Task<PageHeader> CreateHeaderAsync(PageHeader header);
        Task<PageHeader> EditHeaderAsync(PageHeader header);
        Task DeleteHeaderAsync(int id);
        Task<bool> StubInUseAsync(PageHeader header);
        Task<DataWithCount<ICollection<PageLayout>>> GetPaginatedLayoutListForHeaderAsync(
            int headerId, BaseFilter filter);
        Task<PageLayout> CreateLayoutAsync(PageLayout layout);
        Task<PageLayout> EditLayoutAsync(PageLayout layout);
        Task DeleteLayoutAsync(int id);
        Task<PageLayout> GetLayoutByIdAsync(int id);
        Task<PageLayout> GetLayoutDetailsAsync(int id);
        Task<PageLayoutText> SetLayoutTextAsync(PageLayoutText layoutText);
        Task<PageItem> CreateItemAsync(PageItem pageItem);
        Task<PageItem> EditItemAsync(PageItem pageItem);
        Task DeleteItemAsync(int pageItemId);
        Task UpdateItemSortOrder(int id, bool increase);
        Task<PageLayoutText> GetTextByLayoutAndLanguageAsync(int layoutId, int languageId);
        Task<PageLayout> GetLayoutForItemAsync(int itemId);
        Task<PageItem> GetItemByIdAsync(int id);
        Task DeleteItemNoSaveAsync(int pageItemId, bool ignoreSort = false);
        Task<PageLayout> CloneLayoutAsync(int pageHeaderId, int layoutId, string clonedName);
    }
}
