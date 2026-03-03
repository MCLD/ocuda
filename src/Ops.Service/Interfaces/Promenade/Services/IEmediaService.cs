using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEmediaService
    {
        Task AddGroupSegmentAsync(EmediaGroup group);

        Task<Emedia> CreateAsync(Emedia emedia);

        Task<EmediaGroup> CreateGroupAsync(EmediaGroup group);

        Task DeleteAsync(int id);

        Task DeleteGroupAsync(int id);

        Task DeleteGroupSegmentAsync(int groupId);

        Task<Emedia> EditAsync(Emedia emedia);

        Task<EmediaGroup> EditGroupAsync(EmediaGroup group);

        Task EnsureSlugsAsync();

        Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId);

        Task<ICollection<string>> GetEmediaLanguagesAsync(int id);

        Task<EmediaGroup> GetGroupByIdAsync(int id);

        Task<EmediaGroup> GetGroupIncludingSegmentAsync(int id);

        Task<EmediaGroup> GetGroupUsingSegmentAsync(int segmentId);

        Task<Emedia> GetIncludingGroupAsync(int id);

        Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(BaseFilter filter);

        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(int emediaId,
            BaseFilter filter);

        Task<ICollection<Subject>> GetSubjectsForEmediaAsync(int emediaId);

        Task<EmediaText> GetTextByEmediaAndLanguageAsync(int emediaId, int languageId);

        Task ImportItemsAsync(int groupId, IEnumerable<Ocuda.Models.ESourceImport> importData);

        Task SetEmediaTextAsync(EmediaText emediaText);

        Task UpdateCategoriesAsync(int emediaId, ICollection<int> categoryIds);

        Task UpdateGroupSortOrder(int id, bool increase);

        Task UpdateSubjectsAsync(int emediaId, ICollection<int> subjectIds);
    }
}