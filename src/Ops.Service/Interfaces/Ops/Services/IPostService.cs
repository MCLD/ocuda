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

        Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId);

        Task<Category> GetCategoryByIdAsync(int id);

        Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId, int sectionId);

        Category GetCategoryByStub(string stub,int sectionId);

        Task<DataWithCount<ICollection<Category>>> GetPaginatedCategoryListAsync(
            BaseFilter filter, int sectionId);

        Task<DataWithCount<ICollection<Post>>> GetSectionCategoryPaginatedPostListAsync(
            BaseFilter filter, int sectionId, int categoryId);

        Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedPostsAsync(
            BaseFilter filter, int sectionId);

        Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId);

        Task CreatePost(Post post);

        Task RemovePost(Post post);

        Task UpdatePost(Post post);
    }
}
