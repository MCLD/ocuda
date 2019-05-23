using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
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
        private readonly IPostCategoryRepository _postCategoryRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;

        public PostService(ILogger<PostService> logger,
            IPostCategoryRepository postCategoryRepository,
            IPostRepository postRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _postCategoryRepository = postCategoryRepository
                ?? throw new ArgumentNullException(nameof(postCategoryRepository));
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

        public async Task<Post> GetByStubAndCategoryIdAsync(string stub, int categoryId)
        {
            return await _postRepository.GetByStubAndCategoryIdAsync(
                stub?.Trim().ToLower(), categoryId);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _postRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Post> CreateAsync(int currentUserId, Post post)
        {
            post.Title = post.Title?.Trim();
            post.Stub = post.Stub?.Trim().ToLower();
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

            currentPost.Title = post.Title?.Trim();
            currentPost.Stub = post.Stub?.Trim().ToLower();
            currentPost.Content = post.Content;
            currentPost.IsDraft = post.IsDraft;
            currentPost.IsPinned = post.IsPinned;

            if (currentPost.IsDraft)
            {
                currentPost.Stub = post.Stub?.Trim().ToLower();
                if (!post.IsDraft)
                {
                    currentPost.IsDraft = false;
                }
            }

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

        public async Task<bool> StubInUseAsync(Post post)
        {
            return await _postRepository.StubInUseAsync(post);
        }

        private async Task ValidatePostAsync(Post post)
        {
            var message = string.Empty;

            if (!post.IsDraft && await _postRepository.StubInUseAsync(post))
            {
                message = $"Stub '{post.Stub}' already exists in that category.";
                _logger.LogWarning(message, post.Title, post.PostCategoryId);
                throw new OcudaException(message);
            }
        }

        public async Task<PostCategory> GetCategoryByIdAsync(int id)
        {
            return await _postCategoryRepository.FindAsync(id);
        }

        public async Task<IEnumerable<PostCategory>> GetCategoriesBySectionIdAsync(int sectionId)
        {
            return await _postCategoryRepository.GetBySectionIdAsync(sectionId);
        }

        public async Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedCategoryList(
            BlogFilter filter)
        {
            return await _postCategoryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<PostCategory> CreateCategoryAsync(int currentUserId, PostCategory category)
        {
            category.Name = category.Name?.Trim();
            category.CreatedAt = DateTime.Now;
            category.CreatedBy = currentUserId;

            await _postCategoryRepository.AddAsync(category);
            await _postCategoryRepository.SaveAsync();

            return category;
        }

        public async Task<PostCategory> EditCategoryAsync(PostCategory category)
        {
            var currentCategory = await _postCategoryRepository.FindAsync(category.Id);

            currentCategory.Name = category.Name?.Trim();

            _postCategoryRepository.Update(currentCategory);
            await _postCategoryRepository.SaveAsync();

            return currentCategory;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            _postCategoryRepository.Remove(id);
            await _postCategoryRepository.SaveAsync();
        }
    }
}
