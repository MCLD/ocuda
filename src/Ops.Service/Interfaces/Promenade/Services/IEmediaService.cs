﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface IEmediaService
    {
        Task DeleteAsync(int id);
        Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(BaseFilter filter);
        Task DeleteGroupAsync(int id);
        Task<EmediaGroup> CreateGroupAsync(EmediaGroup group);
        Task UpdateGroupSortOrder(int id, bool increase);
        Task<EmediaGroup> EditGroupAsync(EmediaGroup group);
        Task<EmediaGroup> GetGroupByIdAsync(int id);
        Task<Emedia> CreateAsync(Emedia emedia);
        Task<Emedia> EditAsync(Emedia emedia);
        Task<EmediaText> GetTextByEmediaAndLanguageAsync(int emediaId, int languageId);
        Task<Emedia> GetIncludingGroupAsync(int id);
        Task SetEmediaTextAsync(EmediaText emediaText);
        Task UpdateCategoriesAsync(int emediaId, ICollection<int> categoryIds);
        Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId);
        Task<ICollection<string>> GetEmediaLanguagesAsync(int id);
        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(int emediaId,
            BaseFilter filter);
        Task AddGroupSegmentAsync(EmediaGroup group);
        Task<EmediaGroup> GetGroupIncludingSegmentAsync(int id);
        Task DeleteGroupSegmentAsync(int groupId);
        Task<EmediaGroup> GetGroupUsingSegmentAsync(int segmentId);
    }
}
