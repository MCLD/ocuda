using System;
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
        private readonly IPostCategoryRepository _postCategoryRepository;

        public PostService(ILogger<PostService> logger,
            IPostRepository postRepository,
            IPostCategoryRepository postCategoryRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _postRepository = postRepository
                ?? throw new ArgumentNullException(nameof(postRepository));
            _postCategoryRepository = postCategoryRepository
                ?? throw new ArgumentNullException(nameof(postCategoryRepository));
        }

        public async Task<Post> GetPostById(int id)
        {
            return await _postRepository.FindAsync(id);
        }

        public async Task<PostCategory> GetPostCategoryByIdAsync(int id)
        {
            return await _postCategoryRepository.FindAsync(id);
        }

        public PostCategory GetPostCategoryByStub(string stub)
        {
            return _postCategoryRepository.GetPostCategoryByStub(stub);
        }

        public async Task<List<PostCategory>> GetPostCategoriesBySectionIdAsync(int sectionId)
        {
            return await _postCategoryRepository.GetPostsBySectionIdAsync(sectionId);
        }

        public async Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId)
        {
            return await _postRepository.GetPostsByCategoryIdAsync(categoryId);
        }

        public async Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId)
        {
            var categories = await GetPostCategoriesBySectionIdAsync(sectionId);
            var posts = new List<Post>();
            foreach (var category in categories)
            {
                foreach (var post in await GetPostsByCategoryIdAsync(category.Id))
                {
                    posts.Add(post);
                }
            }
            return posts.OrderByDescending(_ => _.PublishedAt).Take(take).ToList();
        }
        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedSectionPostsAsync(
            BaseFilter filter, int sectionId)
        {
            var categories = await GetPostCategoriesBySectionIdAsync(sectionId);
            return await _postRepository.GetSectionPaginatedListAsync(filter, categories);
        }

        public async Task<DataWithCount<ICollection<Post>>> GetPaginatedPostListAsync(
            BaseFilter filter, int categoryId)
        {
            return await _postRepository.GetPaginatedListAsync(filter,categoryId);
        }

        public async Task<DataWithCount<ICollection<PostCategory>>> GetPaginatedPostCategoryListAsync(
            BaseFilter filter, int sectionId)
        {
            return await _postCategoryRepository.GetPaginatedListAsync(filter, sectionId);
        }
    }
}
