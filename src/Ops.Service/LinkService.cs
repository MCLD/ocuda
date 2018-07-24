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
    public class LinkService : ILinkService
    {
        private readonly ILogger<LinkService> _logger;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILinkRepository _linkRepository;
        private readonly ISectionRepository _sectionRepository;
        private readonly IUserRepository _userRepository;

        public LinkService(ILogger<LinkService> logger,
            ICategoryRepository categoryRepository,
            ILinkRepository linkRepository,
            ISectionRepository sectionRepository,
            IUserRepository userRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _categoryRepository = categoryRepository
                ?? throw new ArgumentNullException(nameof(categoryRepository));
            _linkRepository = linkRepository
                ?? throw new ArgumentNullException(nameof(linkRepository));
            _sectionRepository = sectionRepository
                ?? throw new ArgumentNullException(nameof(sectionRepository));
            _userRepository = userRepository
                ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<int> GetLinkCountAsync()
        {
            return await _linkRepository.CountAsync();
        }

        public async Task<ICollection<Link>> GetLinksAsync()
        {
            return await _linkRepository.ToListAsync(_ => _.Name);
        }

        public async Task<Link> GetByIdAsync(int id)
        {
            return await _linkRepository.FindAsync(id);
        }

        public async Task<Link> GetByNameAndSectionIdAsync(string name, int sectionId)
        {
            return await _linkRepository.GetByNameAndSectionIdAsync(name, sectionId);
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _linkRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Link> CreateAsync(int currentUserId, Link link)
        {
            link.CreatedAt = DateTime.Now;
            link.CreatedBy = currentUserId;

            await ValidateLinkAsync(link);

            await _linkRepository.AddAsync(link);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task<Link> EditAsync(Link link)
        {
            var currentLink = await _linkRepository.FindAsync(link.Id);
            currentLink.Name = link.Name;
            currentLink.Url = link.Url;
            currentLink.CategoryId = link.CategoryId;
            currentLink.IsFeatured = link.IsFeatured;

            await ValidateLinkAsync(currentLink);

            _linkRepository.Update(currentLink);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task DeleteAsync(int id)
        {
            _linkRepository.Remove(id);
            await _linkRepository.SaveAsync();
        }

        public async Task ValidateLinkAsync(Link link)
        {
            var message = string.Empty;
            var section = await _sectionRepository.FindAsync(link.SectionId);

            if (section == null)
            {
                message = $"SectionId '{link.SectionId}' is not a valid section.";
                _logger.LogWarning(message, link.SectionId);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(link.Name))
            {
                message = $"Link name cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (string.IsNullOrWhiteSpace(link.Url))
            {
                message = $"Link URL cannot be empty.";
                _logger.LogWarning(message);
                throw new OcudaException(message);
            }

            if (link.CategoryId.HasValue)
            {
                var category = await _categoryRepository.FindAsync(link.CategoryId.Value);
                if (category == null)
                {
                    message = $"CategoryId '{link.CategoryId}' is not valid.";
                    _logger.LogWarning(message);
                    throw new OcudaException(message);
                }
            }

            var creator = await _userRepository.FindAsync(link.CreatedBy);
            if (creator == null)
            {
                message = $"Created by invalid User Id: {link.CreatedBy}";
                _logger.LogWarning(message, link.CreatedBy);
                throw new OcudaException(message);
            }
        }
    }
}
