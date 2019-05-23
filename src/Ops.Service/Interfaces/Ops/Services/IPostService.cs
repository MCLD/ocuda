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
        Task<int> GetPostCountAsync();
        Task<ICollection<Post>> GetPostsAsync(int skip = 0, int take = 5);
        Task<Post> GetByIdAsync(int id);
        Task<Post> GetByStubAndCategoryIdAsync(string stub, int categoryId);
        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter);
        Task<Post> CreateAsync(int currentUserId, Post post);
        Task<Post> EditAsync(Post post);
        Task DeleteAsync(int id);
        Task<bool> StubInUseAsync(Post post);
        Task<PostCategory> GetCategoryByIdAsync(int id);
        Task<IEnumerable<PostCategory>> GetCategoriesBySectionIdAsync(int sectionId);
        Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedCategoryList(BlogFilter filter);
        Task<PostCategory> CreateCategoryAsync(int currentUserId, PostCategory category);
        Task<PostCategory> EditCategoryAsync(PostCategory category);
        Task DeleteCategoryAsync(int id);
    }
}
