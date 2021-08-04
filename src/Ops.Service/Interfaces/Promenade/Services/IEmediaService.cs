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
        Task<Emedia> GetByStubAsync(string emediaStub);
        Task<ICollection<EmediaCategory>> GetEmediaCategoriesById(int emediaId);
        Task UpdateEmediaCategoryAsync(List<int> newCategoryIds, int emediaId);
        Task<ICollection<EmediaCategory>> GetEmediaCategoriesByCategoryId(int categoryId);
        Task AddEmedia(Emedia emedia);
        Task UpdateEmedia(Emedia emedia);
        Task DeleteAsync(int id);
        Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListAsync(
            BaseFilter filter);
        Task<DataWithCount<ICollection<EmediaGroup>>> GetPaginatedGroupListAsync(BaseFilter filter);
        Task DeleteGroupAsync(int id);
        Task<EmediaGroup> CreateGroupAsync(EmediaGroup group);
    }
}
