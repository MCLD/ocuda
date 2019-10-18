using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPostService
    {
        Task<Post> GetPostById(int id);

        Task<List<PostCategory>> GetPostCategoriesBySectionIdAsync(int sectionId);

        Task<PostCategory> GetPostCategoryByIdAsync(int id);

        Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId);

        PostCategory GetPostCategoryByStub(string stub);

        Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedPostCategoryListAsync(
            BaseFilter filter, int sectionId);

        Task<DataWithCount<ICollection<Post>>> GetPaginatedPostListAsync(
            BaseFilter filter, int categoryId);

        Task<DataWithCount<ICollection<Post>>> GetPaginatedSectionPostsAsync(
            BaseFilter filter, int sectionId);

        Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId);
    }
}
