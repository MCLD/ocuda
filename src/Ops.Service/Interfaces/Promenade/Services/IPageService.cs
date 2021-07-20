using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IPageService
    {
        Task<PageLayout> CloneLayoutAsync(int pageHeaderId, int layoutId, string clonedName);

        Task<Page> CreateAsync(Page page);

        Task<PageHeader> CreateHeaderAsync(PageHeader header);

        Task<PageItem> CreateItemAsync(PageItem pageItem);

        Task<PageLayout> CreateLayoutAsync(PageLayout layout);

        Task DeleteAsync(Page page);

        Task DeleteHeaderAsync(int id);

        Task DeleteItemAsync(int pageItemId);

        Task DeleteItemNoSaveAsync(int pageItemId, bool ignoreSort = false);

        Task DeleteLayoutAsync(int id);

        Task<Page> EditAsync(Page page);

        Task<PageHeader> EditHeaderAsync(PageHeader header);

        Task<PageItem> EditItemAsync(PageItem pageItem);

        Task<PageLayout> EditLayoutAsync(PageLayout layout);

        Task<Page> GetByHeaderAndLanguageAsync(int headerId, int languageId);

        Task<PageHeader> GetHeaderByIdAsync(int id);

        Task<PageItem> GetItemByIdAsync(int id);

        Task<PageLayout> GetLayoutByIdAsync(int id);

        Task<PageLayout> GetLayoutDetailsAsync(int id);

        Task<PageLayout> GetLayoutForItemAsync(int itemId);

        Task<DataWithCount<ICollection<PageHeader>>> GetPaginatedHeaderListAsync(PageFilter filter);

        Task<DataWithCount<ICollection<PageLayout>>> GetPaginatedLayoutListForHeaderAsync(
            int headerId, BaseFilter filter);

        Task<PageLayoutText> GetTextByLayoutAndLanguageAsync(int layoutId, int languageId);

        Task<PageLayoutText> SetLayoutTextAsync(PageLayoutText layoutText);

        Task<bool> StubInUseAsync(PageHeader header);

        Task UpdateItemSortOrder(int id, bool increase);
    }
}