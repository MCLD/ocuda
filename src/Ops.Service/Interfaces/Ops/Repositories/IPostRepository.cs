using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPostRepository : IRepository<Post, int>
    {
        Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId);

        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(
            BaseFilter filter, int categoryId);
        Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedListAsync(
            BaseFilter filter, List<PostCategory> categories);
    }
}
