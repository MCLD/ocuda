using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface IPostRepository : IOpsRepository<Post, int>
    {
        Task<List<Post>> GetPostsBySectionCategoryIdAsync(int categoryId, int sectionId);

        Task<Post> GetSectionPostBySlugAsync(string stub, int sectionId);

        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter);

        Task<List<Post>> GetTopSectionPostsAsync(int sectionId, int count);

        Task<List<PostCategory>> GetPostCategoriesAsync(int id);

        Task AddPostCategoriesAsync(List<int> categories, int postId);

        Task DeletePostCategoriesAsync(List<int> categories, int postId);
    }
}