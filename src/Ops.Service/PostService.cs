using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class PostService : BaseService<PostService>, IPostService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IPostRepository _postRepository;
        private readonly ISectionService _sectionService;
        private readonly IUserService _userService;

        public PostService(ILogger<PostService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICategoryRepository categoryRepository,
            IPostRepository postRepository,
            ISectionService sectionService,
            IUserService userService)
            : base(logger, httpContextAccessor)
        {
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _postRepository = postRepository
                ?? throw new ArgumentNullException(nameof(postRepository));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<Post> CreatePostAsync(Post post)
        {
            if (post != null)
            {
                post.Content = post.Content?.Trim();
                post.Title = post.Title?.Trim();
                post.Slug = post.Slug?.Trim();
                post.CreatedAt = DateTime.Now;
                post.CreatedBy = GetCurrentUserId();

                await _postRepository.AddAsync(post);
                await _postRepository.SaveAsync();
            }

            return post;
        }

        public async Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId)
        {
            return await _categoryRepository.GetCategoriesBySectionIdAsync(sectionId);
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedPostsAsync(
            BlogFilter filter)
        {
            var postsWithCount = await _postRepository.GetPaginatedListAsync(filter);

            // populate additional info if it's a limited number of posts
            if (postsWithCount.Count <= new BlogFilter().Take)
            {
                foreach (var post in postsWithCount.Data)
                {
                    if (post.CreatedByUser == null)
                    {
                        (post.CreatedByName, post.CreatedByUsername)
                            = await _userService.GetNameUsernameAsync(post.CreatedBy);
                    }
                    if (string.IsNullOrEmpty(post.SectionName))
                    {
                        var section = await _sectionService.GetByIdAsync(post.SectionId);
                        post.SectionName = section.Name;
                        post.SectionSlug = section.Slug;
                    }
                }
            }

            return postsWithCount;
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _postRepository.FindAsync(id);
        }

        public async Task<List<PostCategory>> GetPostCategoriesByIdAsync(int postId)
        {
            return await _postRepository.GetPostCategoriesAsync(postId);
        }

        public async Task<List<PostCategory>> GetPostCategoriesByIdsAsync(List<int> postIds)
        {
            var postList = new List<PostCategory>();
            foreach (var id in postIds)
            {
                var postcats = await _postRepository.GetPostCategoriesAsync(id);
                postList.AddRange(postcats);
            }
            return postList;
        }

        public async Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId, int sectionId)
        {
            return await _postRepository.GetPostsBySectionCategoryIdAsync(categoryId, sectionId);
        }

        public async Task<Category> GetSectionCategoryBySlugAsync(string slug, int sectionId)
        {
            var category = await _categoryRepository.GetCategoryBySlugAsync(slug);

            return await _categoryRepository.SectionHasCategoryAsync(category.Id, sectionId) ? category : null;
        }

        public async Task<Post> GetSectionPostBySlugAsync(string slug, int sectionId)
        {
            return await _postRepository.GetSectionPostBySlugAsync(slug, sectionId);
        }

        public async Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId)
        {
            return await _postRepository.GetTopSectionPostsAsync(sectionId, take);
        }

        public async Task RemovePostAsync(Post post)
        {
            if (post != null)
            {
                var currentCats = await _postRepository.GetPostCategoriesAsync(post.Id);
                await _postRepository.DeletePostCategoriesAsync(
                    currentCats.Select(_ => _.CategoryId).ToList(), post.Id);
                _postRepository.Remove(post);

                await _postRepository.SaveAsync();
            }
        }

        public async Task UpdatePostAsync(Post post)
        {
            if (post != null)
            {
                var oldPost = await _postRepository.FindAsync(post.Id);
                oldPost.ShowOnHomePage = post.ShowOnHomePage;
                oldPost.Content = post.Content?.Trim();
                oldPost.Title = post.Title?.Trim();
                oldPost.Slug = post.Slug?.Trim();
                oldPost.UpdatedAt = DateTime.Now;
                oldPost.UpdatedBy = GetCurrentUserId();

                _postRepository.Update(oldPost);
                await _postRepository.SaveAsync();
            }
        }

        public async Task UpdatePostCategoriesAsync(List<int> newCategoryIds, int postId)
        {
            var currentCats = await _postRepository.GetPostCategoriesAsync(postId);
            var currentCatIds = currentCats.Select(_ => _.CategoryId).ToList();

            if (newCategoryIds == null)
            {
                newCategoryIds = new List<int>();
            }
            var catsToDelete = currentCatIds.Except(newCategoryIds).ToList();
            var catsToAdd = newCategoryIds.Except(currentCatIds).ToList();

            await _postRepository.DeletePostCategoriesAsync(catsToDelete, postId);
            await _postRepository.AddPostCategoriesAsync(catsToAdd, postId);
        }
    }
}