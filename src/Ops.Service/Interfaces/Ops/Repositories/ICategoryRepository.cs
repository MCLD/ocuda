using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId);

        Category GetCategoryByStub(string stub);

        Task<DataWithCount<ICollection<Category>>> GetPaginatedListAsync(
            BaseFilter filter, int sectionId);

        Task<bool> SectionHasCategoryAsync(int categoryId, int sectionId);

    }
}
