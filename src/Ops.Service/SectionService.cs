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
    public class SectionService : ISectionService
    {
        private readonly ILogger<SectionService> _logger;
        private readonly ILinkLibraryRepository _linkLibraryRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;

        public SectionService(ILogger<SectionService> logger,
            ILinkLibraryRepository linkLibraryRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _logger = logger
               ?? throw new ArgumentNullException(nameof(logger));
            _linkLibraryRepository = linkLibraryRepository 
                ?? throw new ArgumentNullException(nameof(linkLibraryRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
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

        public async Task<IEnumerable<SectionWithNavigation>> GetNavigationAsync()
        {
            return await _sectionRepository.GetNavigationSectionsAsync();
        }

        public async Task<Section> GetByIdAsync(int id)
        {
            return await _sectionRepository.FindAsync(id);
        }

        public async Task<bool> IsValidPathAsync(string path)
        {
            return await _sectionRepository.IsValidPathAsync(path?.Trim().ToLower());
        }

        public async Task<Section> GetByPathAsync(string path)
        {
            return await _sectionRepository.GetByPathAsync(path?.Trim().ToLower());
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
            section.Name = section.Name?.Trim();
            section.Path = section.Path?.Trim().ToLower();
            section.CreatedAt = DateTime.Now;
            section.CreatedBy = currentUserId;

            await ValidateSectionAsync(section);

            await _sectionRepository.AddAsync(section);

            var navigationLinkLibrary = new LinkLibrary
            {
                CreatedAt = DateTime.Now,
                CreatedBy = currentUserId,
                IsNavigation = true,
                Name = "Navigation",
                Section = section
            };
            await _linkLibraryRepository.AddAsync(navigationLinkLibrary);


            await _sectionRepository.SaveAsync();

            return section;
        }

        public async Task<Section> EditAsync(Section section)
        {
            var currentSection = await _sectionRepository.FindAsync(section.Id);
            currentSection.Name = section.Name?.Trim();
            currentSection.Path = section.Path?.Trim().ToLower();
            currentSection.Icon = section.Icon;
            currentSection.SortOrder = section.SortOrder;
            currentSection.IsNavigation = section.IsNavigation;

            await ValidateSectionAsync(currentSection);

            _sectionRepository.Update(currentSection);
            await _sectionRepository.SaveAsync();
            return currentSection;
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

            if (defaultSection != null && defaultSection.Id != section.Id 
                && string.IsNullOrWhiteSpace(section.Path))
            {
                message = $"Section path cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (await _sectionRepository.IsDuplicateNameAsync(section))
            {
                message = $"Section '{section.Name}' already exists.";
                _logger.LogWarning(message, section.Name);
                throw new OcudaException(message);
            }

            if (await _sectionRepository.IsDuplicatePathAsync(section))
            {
                message = $"Section path '{section.Path }' already exists.";
                _logger.LogWarning(message, section.Path);
                throw new OcudaException(message);
            }
        }
    }
}
