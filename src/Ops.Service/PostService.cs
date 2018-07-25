using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class PostService : IPostService
    {
        private readonly ILogger _logger;
        private readonly IPostRepository _postRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;

        public PostService(ILogger<PostService> logger,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _postRepository = postRepository 
                ?? throw new ArgumentNullException(nameof(postRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
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

        public async Task<Post> GetByStubAsync(string stub)
        {
            return await _postRepository.GetByStubAsync(stub.Trim().ToLower());
        }

        public async Task<Post> GetByStubAndSectionIdAsync(string stub, int sectionId)
        {
            return await _postRepository.GetByStubAndSectionIdAsync(stub.Trim().ToLower(), sectionId);
        }

        public async Task<Post> GetByTitleAndSectionIdAsync(string title, int sectionId)
        {
            return await _postRepository.GetByTitleAndSectionIdAsync(title.Trim(), sectionId);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _postRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Post> CreateAsync(int currentUserId, Post post)
        {
            post.Title = post.Title.Trim();
            post.Stub = post.Stub.Trim().ToLower();
            post.CreatedAt = DateTime.Now;
            post.CreatedBy = currentUserId;

            await ValidatePostAsync(post);

            await _postRepository.AddAsync(post);
            await _postRepository.SaveAsync();

            return post;
        }

        public async Task<Post> EditAsync(Post post)
        {
            var currentPost = await _postRepository.FindAsync(post.Id);
            currentPost.Title = post.Title.Trim();
            currentPost.Stub = post.Stub.Trim().ToLower();
            currentPost.Content = post.Content;
            currentPost.IsDraft = post.IsDraft;
            currentPost.IsPinned = post.IsPinned;
            currentPost.ShowOnHomepage = post.ShowOnHomepage;

            await ValidatePostAsync(post);

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
            return await _postRepository.StubInUseAsync(stub.Trim().ToLower(), sectionId);
        }

        public async Task ValidatePostAsync(Post post)
        {
            var message = string.Empty;
            var section = await _sectionRepository.FindAsync(post.SectionId);

            if (section == null)
            {
                message = $"SectionId '{post.SectionId}' is not a valid section.";
                _logger.LogWarning(message, post.SectionId);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(post.Title))
            {
                message = $"Post title cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(post.Stub))
            {
                message = $"Post stub cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            var stubInUse = await _postRepository.StubInUseAsync(post.Stub, post.SectionId);

            if (!post.IsDraft && stubInUse)
            {
                message = $"Stub '{post.Stub}' already exists in '{section.Name}'.";
                _logger.LogWarning(message, post.Title, post.SectionId);
                throw new OcudaException(message);
            }

            var creator = await _userRepository.FindAsync(post.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {post.CreatedBy}";
                _logger.LogWarning(message, post.CreatedBy);
                throw new OcudaException(message);
            }
        }
    }
}
