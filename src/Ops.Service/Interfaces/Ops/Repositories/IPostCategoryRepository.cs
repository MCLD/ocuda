using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPostCategoryRepository : IRepository<PostCategory, int>
    {
        Task<List<PostCategory>> GetPostsBySectionIdAsync(int sectionId);

        PostCategory GetPostCategoryByStub(string stub);

        Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedListAsync(
            BaseFilter filter, int sectionId);
    }
}
