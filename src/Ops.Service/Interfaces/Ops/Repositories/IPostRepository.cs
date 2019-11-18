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
        Task<List<Post>> GetPostsBySectionCategoryIdAsync(int categoryId, int sectionId);

        Task<DataWithCount<ICollection<Post>>> GetSectionCategoryPaginatedListAsync(
            BaseFilter filter, int sectionId, int categoryId);

        Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedListAsync(
            BaseFilter filter, int sectionId);

        Task<List<Post>> GetTopSectionPosts(int sectionId, int count);

        Task<List<PostCategory>> GetPostCategory(int id);

        Post GetSectionPostByStub(string stub, int sectionId);

        Task AddPostCategory(List<int> categories, int postId);

        Task DeletePostCategory(List<int> categories, int postId);
    }
}
