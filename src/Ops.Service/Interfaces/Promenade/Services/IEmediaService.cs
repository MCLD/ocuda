using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

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
    }
}
