using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class PostService
    {
        private readonly InsertSampleDataService _insertSampleDataService;
        private readonly IPostRepository _postRepository;

        public PostService(InsertSampleDataService insertSampleDataService,
            IPostRepository postRepository)
        {
            _postRepository = postRepository 
                ?? throw new ArgumentNullException(nameof(postRepository));
            _insertSampleDataService = insertSampleDataService
                ?? throw new ArgumentNullException(nameof(insertSampleDataService));
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BaseFilter filter)
        {
            return new DataWithCount<ICollection<Post>>
            {
                Count = await _postRepository.CountAsync(),
                Data = await _postRepository.ToListAsync(filter.Skip.Value, filter.Take.Value,
                _ => _.IsPinned == false, _ => _.CreatedAt)
            };
        }
 
        public async Task<int> GetPostCountAsync()
        {
            return await _postRepository.CountAsync();
        }

        public async Task<ICollection<Post>> GetPostsAsync(int skip = 0, int take = 5)
        {
            // TODO modify this to do descending (most recent first)
            return await _postRepository.ToListAsync(skip, take, _ => _.CreatedAt);
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _postRepository.FindAsync(id);
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            post.CreatedAt = DateTime.Now;
            await _postRepository.AddAsync(post);
            await _postRepository.SaveAsync();

            return post;
        }

        public async Task<Post> EditPostAsync(Post post)
        {
            // TODO fix edit logic
            _postRepository.Update(post);
            await _postRepository.SaveAsync();
            return post;
        }

        public async Task DeletePostAsync(int id)
        {
            _postRepository.Remove(id);
            await _postRepository.SaveAsync();
        }
    }
}
