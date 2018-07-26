using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPostService
    {
        Task<int> GetPostCountAsync();
        Task<ICollection<Post>> GetPostsAsync(int skip = 0, int take = 5);
        Task<Post> GetByIdAsync(int id);
        Task<Post> GetByStubAsync(string stub);
        Task<Post> GetByStubAndSectionIdAsync(string stub, int sectionId);
        Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter);
        Task<Post> CreateAsync(int currentUserId, Post post);
        Task<Post> EditAsync(Post post);
        Task DeleteAsync(int id);
        Task<bool> StubInUseAsync(Post post);
    }
}
