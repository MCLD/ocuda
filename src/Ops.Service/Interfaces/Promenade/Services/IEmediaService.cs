using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEmediaService
    {
        Task<ICollection<Emedia>> GetAllEmedia();
        Task<ICollection<EmediaCategory>> GetEmediaCategoriesById(int emediaId);
        Task UpdateEmediaCategoryAsync(List<int> newCategoryIds, int emediaId);
        Task<ICollection<EmediaCategory>> GetEmediaCategoriesByCategoryId(int categoryId);
        Task DeleteAsync(int id);
        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListAsync(
            BaseFilter filter);
        Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(BaseFilter filter);
        Task DeleteGroupAsync(int id);
        Task<EmediaGroup> CreateGroupAsync(EmediaGroup group);
        Task UpdateGroupSortOrder(int id, bool increase);
        Task<EmediaGroup> EditGroupAsync(EmediaGroup group);
        Task<EmediaGroup> GetGroupByIdAsync(int id);
        Task<Emedia> CreateAsync(Emedia emedia);
        Task<Emedia> EditAsync(Emedia emedia);
        Task<Emedia> GetByIdAsync(int id);
        Task<EmediaText> GetTextByEmediaAndLanguageAsync(int emediaId, int languageId);
        Task<Emedia> GetIncludingGroupAsync(int id);
    }
}
