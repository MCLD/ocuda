using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPostService
    {
        Task<Post> GetPostByIdAsync(int id);

        Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId);

        Task<Category> GetCategoryByIdAsync(int id);

        Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId, int sectionId);

        Task<Category> GetSectionCategoryByStubAsync(string stub, int sectionId);

        Task<DataWithCount<ICollection<Post>>> GetSectionCategoryPaginatedPostListAsync(
            BaseFilter filter, int sectionId, int categoryId);

        Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedPostsAsync(
            BaseFilter filter, int sectionId);

        Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId);

        Task<List<PostCategory>> GetPostCategoriesByIdsAsync(List<int> postIds);

        Task<List<PostCategory>> GetPostCategoriesByIdAsync(int postId);

        Task CreatePostAsync(Post post, int userId);

        Task RemovePostAsync(Post post);

        Task UpdatePostAsync(Post post);

        Task<Post> GetSectionPostByStubAsync(string stub, int sectionId);

        Task UpdatePostCategoriesAsync(List<int> newCategoryIds, int postId);
    }
}
