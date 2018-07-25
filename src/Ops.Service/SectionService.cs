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
    public class SectionService : ISectionService
    {
        private readonly ILogger<SectionService> _logger;
        private readonly ISectionRepository _sectionRepository;
        private readonly ICategoryService _categoryService;
        private readonly IUserRepository _userRepository;

        public SectionService(ILogger<SectionService> logger,
            ISectionRepository sectionRepository,
            IUserRepository userRepository,
            ICategoryService categoryService)
        {
            _logger = logger
               ?? throw new ArgumentNullException(nameof(logger));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
        }

        /// <summary>
        /// Ensure the default section exists.
        /// </summary>
        public async Task EnsureDefaultSectionAsync(int sysadminId)
        {
            var defaultSection = await _sectionRepository.GetDefaultSectionAsync();
            if (defaultSection == null)
            {
                defaultSection = new Section
                {
                    Name = "Default Section",
                    CreatedAt = DateTime.Now,
                    CreatedBy = sysadminId,
                    SortOrder = 0
                };

                await CreateAsync(sysadminId, defaultSection);
            }
        }

        public async Task<IEnumerable<Section>> GetNavigationAsync()
        {
            return await _sectionRepository.GetNavigationSectionsAsync();
        }

        public async Task<Section> GetByIdAsync(int id)
        {
            return await _sectionRepository.FindAsync(id);
        }

        public async Task<bool> IsValidPathAsync(string path)
        {
            return await _sectionRepository.IsValidPathAsync(path.Trim().ToLower());
        }

        public async Task<Section> GetByNameAsync(string name)
        {
            return await _sectionRepository.GetByNameAsync(name.Trim());
        }

        public async Task<Section> GetByPathAsync(string path)
        {
            return await _sectionRepository.GetByPathAsync(path.Trim().ToLower());
        }

        public async Task<DataWithCount<ICollection<Section>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _sectionRepository.GetPaginatedListAsync(filter);
        }

        public async Task<IEnumerable<Section>> GetSectionsAsync()
        {
            return await _sectionRepository
                .ToListAsync(_ => _.SortOrder);
        }

        public async Task<int> GetSectionCountAsync()
        {
            return await _sectionRepository.CountAsync();
        }

        public async Task<Section> CreateAsync(int currentUserId, Section section)
        {
            section.Name = section.Name.Trim();
            section.Path = section.Path.Trim().ToLower();
            section.CreatedAt = DateTime.Now;
            section.CreatedBy = currentUserId;

            await ValidateSectionAsync(section);

            await _sectionRepository.AddAsync(section);
            await _sectionRepository.SaveAsync();
            await _categoryService.CreateDefaultCategories(currentUserId, section.Id);

            return section;
        }

        public async Task<Section> EditAsync(Section section)
        {
            var currentSection = await _sectionRepository.FindAsync(section.Id);
            currentSection.Name = section.Name.Trim();
            currentSection.Path = section.Path.Trim().ToLower();
            currentSection.Icon = section.Icon;
            currentSection.SortOrder = section.SortOrder;
            currentSection.FeaturedVideoUrl = section.FeaturedVideoUrl;

            await ValidateSectionAsync(currentSection);

            _sectionRepository.Update(currentSection);
            await _sectionRepository.SaveAsync();
            return currentSection;
        }

        public async Task EditFeaturedVideoUrlAsync(int sectionId, string url)
        {
            var currentSection = await _sectionRepository.FindAsync(sectionId);
            currentSection.FeaturedVideoUrl = url;

            _sectionRepository.Update(currentSection);
            await _sectionRepository.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var section = await _sectionRepository.FindAsync(id);
            section.IsDeleted = true;
            _sectionRepository.Update(section);
            await _sectionRepository.SaveAsync();
        }

        public async Task ValidateSectionAsync(Section section)
        {
            var message = string.Empty;
            var defaultSection = await _sectionRepository.GetDefaultSectionAsync();

            if (defaultSection != null && defaultSection.Id != section.Id)
            {
                if (string.IsNullOrWhiteSpace(section.Path))
                {
                    message = $"Section path cannot be empty.";
                    _logger.LogWarning(message);
                    throw new OcudaException(message);
                }
            }

            if (string.IsNullOrWhiteSpace(section.Name))
            {
                message = $"Section name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (await _sectionRepository.IsDuplicateNameAsync(section.Name))
            {
                message = $"Section '{section.Name}' already exists.";
                _logger.LogWarning(message, section.Name);
                throw new OcudaException(message);
            }

            if (await _sectionRepository.IsDuplicatePathAsync(section.Path))
            {
                message = $"Section path '{section.Path }' already exists.";
                _logger.LogWarning(message, section.Path);
                throw new OcudaException(message);
            }

            var creator = await _userRepository.FindAsync(section.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {section.CreatedBy}";
                _logger.LogWarning(message, section.CreatedBy);
                throw new OcudaException(message);
            }
        }
    }
}
