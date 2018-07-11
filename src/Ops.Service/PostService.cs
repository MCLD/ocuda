using System;
using System.Collections.Generic;
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

        public async Task<int> GetPostCountAsync()
        {
            return await _postRepository.CountAsync();
        }

        public async Task<ICollection<Post>> GetPostsAsync(int skip = 0, int take = 5)
        {
            // TODO modify this to do descending (most recent first)
            return await _postRepository.ToListAsync(skip, take, _ => _.CreatedAt);
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            return await _postRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _postRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Post> CreateAsync(Post post)
        {
            post.CreatedAt = DateTime.Now;
            post.CreatedBy = 1; // TODO Set CreatedBy Id
            await _postRepository.AddAsync(post);
            await _postRepository.SaveAsync();

            return post;
        }

        public async Task<Post> EditAsync(Post post)
        {
            var currentPost = await _postRepository.FindAsync(post.Id);
            currentPost.Title = post.Title;
            currentPost.Stub = post.Stub;
            currentPost.Content = post.Content;
            currentPost.IsDraft = post.IsDraft;
            currentPost.IsPinned = post.IsPinned;
            currentPost.ShowOnHomepage = post.ShowOnHomepage;

            _postRepository.Update(currentPost);
            await _postRepository.SaveAsync();
            return currentPost;
        }

        public async Task DeleteAsync(int id)
        {
            _postRepository.Remove(id);
            await _postRepository.SaveAsync();
        }

        public async Task<bool> StubInUseAsync(string stub, int sectionId)
        {
            return await _postRepository.StubInUseAsync(stub, sectionId);
        }
    }
}
