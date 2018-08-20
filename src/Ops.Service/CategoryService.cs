using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryFileTypeRepository _categoryFileTypeRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;

        public CategoryService(ILogger<CategoryService> logger,
            ICategoryRepository categoryRepository,
            ICategoryFileTypeRepository categoryFileTypeRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _categoryFileTypeRepository = categoryFileTypeRepository
                ?? throw new ArgumentNullException(nameof(categoryFileTypeRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<DataWithCount<ICollection<Category>>>
           GetPaginatedCategoryListAsync(BlogFilter filter)
        {
            return await _categoryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<ICollection<Category>> GetCategoriesAsync()
        {
            return await _categoryRepository.ToListAsync(_ => _.Name);
        }

        public async Task<ICollection<Category>> GetBySectionIdAsync(BlogFilter filter, bool isGallery = false)
        {
            return await _categoryRepository.GetBySectionIdAsync(filter, isGallery);
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _categoryRepository.FindAsync(id);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _categoryRepository.GetByNameAsync(name?.Trim());
        }

        public async Task<int> GetCategoryCountAsync()
        {
            return await _categoryRepository.CountAsync();
        }

        public async Task<Category> CreateCategoryAsync(
            int currentUserId, Category category, int[] fileTypeIds)
        {
            category.Name = category.Name?.Trim();
            category.CreatedAt = DateTime.Now;
            category.CreatedBy = currentUserId;

            if (fileTypeIds != null && fileTypeIds.Length > 0)
            {
                var categoryFileTypes = new List<CategoryFileType>();

                foreach (var fileTypeId in fileTypeIds)
                {
                    categoryFileTypes.Add(new CategoryFileType
                    {
                        CreatedAt = category.CreatedAt,
                        CreatedBy = category.CreatedBy,
                        FileTypeId = fileTypeId
                    });
                }

                category.CategoryFileTypes = categoryFileTypes;
            }

            await ValidateCategoryAsync(category);

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveAsync();

            return category;
        }

        public async Task<Category> EditCategoryAsync(
            int currentUserId, int id, string name, bool thumbnail = false, int[] fileTypeIds = null)
        {
            var currentCategory = await _categoryRepository.FindAsync(id);
            currentCategory.Name = name?.Trim();
            currentCategory.ThumbnailRequired = thumbnail;

            var typesToRemove = currentCategory.CategoryFileTypes
                .Where(_ => fileTypeIds.Contains(_.FileTypeId) == false).ToList();

            foreach (var fileType in typesToRemove)
            {
                currentCategory.CategoryFileTypes.Remove(fileType);
                _categoryFileTypeRepository.Remove(fileType);
            }

            var typeIdsToAdd = fileTypeIds.Except(
                currentCategory.CategoryFileTypes.Select(_ => _.FileTypeId));

            foreach (var fileTypeId in typeIdsToAdd)
            {
                var newCategoryFileType = new CategoryFileType
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = currentUserId,
                    FileTypeId = fileTypeId
                };
                currentCategory.CategoryFileTypes.Add(newCategoryFileType);
            }

            await ValidateCategoryAsync(currentCategory);

            _categoryRepository.Update(currentCategory);
            await _categoryRepository.SaveAsync();
            await _categoryFileTypeRepository.SaveAsync();
            
            return currentCategory;
        }

        public async Task DeleteCategoryAsync(int id)
        {
            _categoryRepository.Remove(id);
            _categoryFileTypeRepository.RemoveByCategoryId(id);
            await _categoryRepository.SaveAsync();
        }

        public async Task CreateDefaultCategories(int currentUserId, Section section)
        {
            var defaultFileCategory = new Category
            {
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                CategoryType = CategoryType.File,
                IsDefault = true,
                Name = $"{section.Name} Files",
                SectionId = section.Id
            };

            var defaultLinkCategory = new Category
            {
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                CategoryType = CategoryType.Link,
                IsDefault = true,
                Name = $"{section.Name} Links",
                SectionId = section.Id
            };

            var attachmentCategory = new Category
            {
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                CategoryType = CategoryType.File,
                IsDefault = true,
                IsAttachment = true,
                Name = $"{section.Name} Attachments",
                SectionId = section.Id
            };

            var navigationCategory = new Category
            {
                CreatedBy = currentUserId,
                CreatedAt = DateTime.Now,
                CategoryType = CategoryType.Link,
                IsDefault = true,
                IsNavigation = true,
                Name = "Navigation",
                SectionId = section.Id
            };

            await _categoryRepository.AddAsync(defaultFileCategory);
            await _categoryRepository.AddAsync(defaultLinkCategory);
            await _categoryRepository.AddAsync(attachmentCategory);
            await _categoryRepository.AddAsync(navigationCategory);
            await _categoryRepository.SaveAsync();
        }

        public async Task<Category> GetDefaultAsync(BlogFilter filter)
        {
            return await _categoryRepository.GetDefaultAsync(filter);
        }

        public async Task<Category> GetAttachmentAsync(BlogFilter filter)
        {
            return await _categoryRepository.GetAttachmentAsync(filter);
        }

        public async Task ValidateCategoryAsync(Category category)
        {
            var message = string.Empty;
            var section = await _sectionRepository.FindAsync(category.SectionId);

            if (section == null)
            {
                message = $"SectionId '{category.SectionId}' is not a valid section.";
                _logger.LogWarning(message, category.SectionId);
                throw new OcudaException(message);
            }

            if (!Enum.IsDefined(typeof(CategoryType), category.CategoryType))
            {
                message = $"Category type is invalid.";
                _logger.LogWarning(message, category.CategoryType);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(category.Name))
            {
                message = $"Category name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (await _categoryRepository.IsDuplicateAsync(category))
            {
                message = $"Category '{category.Name}' already exists in '{section.Name}'.";
                _logger.LogWarning(message, category.Name, category.SectionId);
                throw new OcudaException(message);
            }

            var creator = await _userRepository.FindAsync(category.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {category.CreatedBy}";
                _logger.LogWarning(message, category.CreatedBy);
                throw new OcudaException(message);
            }
        }

        public async Task<Category> GetCategoryAndFileTypesByCategoryIdAsync(int categoryId)
        {
            return await _categoryRepository.GetCategoryAndFileTypesByCategoryIdAsync(categoryId);
        }

        public async Task<IEnumerable<int>> GetFileTypeIdsByCategoryIdAsync(int categoryId)
        {
            return await _categoryFileTypeRepository.GetFileTypeIdsByCategoryIdAsync(categoryId);
        }

    }
}
