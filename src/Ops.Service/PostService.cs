﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Service
{
    public class PostService : IPostService
    {
        private readonly ILogger<PostService> _logger;
        private readonly IPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;

        public PostService(ILogger<PostService> logger,
            IPostRepository postRepository,
            ICategoryRepository categoryRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _postRepository = postRepository
                ?? throw new ArgumentNullException(nameof(postRepository));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
        }

        public async Task<Post> GetPostById(int id)
        {
            return await _postRepository.FindAsync(id);
        }

        public async Task CreatePost(Post post)
        {
            if (post != null)
            {
                post.Content = post.Content?.Trim();
                post.Title = post.Title?.Trim();
                post.Stub = post.Stub?.Trim();

                await _postRepository.AddAsync(post);
                await _postRepository.SaveAsync();
            }
        }

        public async Task UpdatePost(Post post)
        {
            if (post != null)
            {
                post.Content = post.Content?.Trim();
                post.Title = post.Title?.Trim();
                post.Stub = post.Stub?.Trim();

                _postRepository.Update(post);
                await _postRepository.SaveAsync();
            }
        }

        public async Task RemovePost(Post post)
        {
            if (post != null)
            {
                _postRepository.Remove(post);
                await _postRepository.SaveAsync();
            }
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public Category GetCategoryByStub(string stub, int sectionId)
        {
            return _categoryRepository.GetCategoryByStub(stub, sectionId);
        }

        public async Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId)
        {
            return await _categoryRepository.GetCategoriesBySectionIdAsync(sectionId);
        }

        public async Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId, int sectionId)
        {
            return await _postRepository.GetPostsBySectionCategoryIdAsync(categoryId, sectionId);
        }

        public async Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId)
        {
            return await _postRepository.GetTopSectionPosts(sectionId,take);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetSectionPaginatedPostsAsync(
            BaseFilter filter, int sectionId)
        {
            return await _postRepository.GetSectionPaginatedListAsync(filter, sectionId);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetSectionCategoryPaginatedPostListAsync(
            BaseFilter filter, int sectionId, int categoryId)
        {
            return await _postRepository.GetSectionCategoryPaginatedListAsync(filter,sectionId,categoryId);
        }

        public async Task<DataWithCount<ICollection<Category>>> GetPaginatedCategoryListAsync(
            BaseFilter filter, int sectionId)
        {
            return await _categoryRepository.GetPaginatedListAsync(filter, sectionId);
        }
    }
}