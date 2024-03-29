﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IPostService
    {
        Task<Post> GetPostByIdAsync(int id);

        Task<List<Category>> GetCategoriesBySectionIdAsync(int sectionId);

        Task<Category> GetCategoryByIdAsync(int id);

        Task<List<Post>> GetPostsByCategoryIdAsync(int categoryId, int sectionId);

        Task<Category> GetSectionCategoryBySlugAsync(string slug, int sectionId);

        Task<DataWithCount<ICollection<Post>>> GetPaginatedPostsAsync(BlogFilter filter);

        Task<List<Post>> GetTopSectionPostsAsync(int take, int sectionId);

        Task<List<PostCategory>> GetPostCategoriesByIdsAsync(List<int> postIds);

        Task<List<PostCategory>> GetPostCategoriesByIdAsync(int postId);

        Task<Post> CreatePostAsync(Post post);

        Task RemovePostAsync(Post post);

        Task UpdatePostAsync(Post post);

        Task<Post> GetSectionPostBySlugAsync(string slug, int sectionId);

        Task UpdatePostCategoriesAsync(List<int> newCategoryIds, int postId);
    }
}
