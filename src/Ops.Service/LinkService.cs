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
    public class LinkService : ILinkService
    {
        private readonly ILogger<LinkService> _logger;
        private readonly ILinkLibraryRepository _linkLibraryRepository;
        private readonly ILinkRepository _linkRepository;

        public LinkService(ILogger<LinkService> logger,
            ILinkLibraryRepository linkLibraryRepository,
            ILinkRepository linkRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _linkRepository = linkRepository
                ?? throw new ArgumentNullException(nameof(linkRepository));
            _linkLibraryRepository = linkLibraryRepository
                ?? throw new ArgumentNullException(nameof(linkLibraryRepository));
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

        public async Task<Link> GetLatestByLibraryIdAsync(int id)
        {
            return await _linkRepository.GetLatestByLibraryIdAsync(id);
        }

        public async Task<List<Link>> GetLinkLibraryLinksAsync(int id)
        {
            return await _linkRepository.GetFileLibraryFilesAsync(id);
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(BlogFilter filter)
        {
            return await _linkRepository.GetPaginatedListAsync(filter);
        }

        public async Task<Link> CreateAsync(int currentUserId, Link link)
        {
            var newLink = new Link
            {
                Name = link.Name?.Trim(),
                Url = link.Url?.Trim(),
                Icon = link.Icon.Trim(),
                LinkLibraryId = link.LinkLibraryId,
                CreatedAt = DateTime.Now,
                CreatedBy = currentUserId
            };

            await _linkRepository.AddAsync(newLink);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task<Link> EditAsync(Link link)
        {
            var currentLink = await _linkRepository.FindAsync(link.Id);

            currentLink.Name = link.Name?.Trim();
            currentLink.Url = link.Url?.Trim();
            currentLink.Icon = link.Icon;

            _linkRepository.Update(currentLink);
            await _linkRepository.SaveAsync();
            return link;
        }

        public async Task DeleteAsync(int id)
        {
            _linkRepository.Remove(id);
            await _linkRepository.SaveAsync();
        }

        public async Task<LinkLibrary> GetLibraryByIdAsync(int id)
        {
            return await _linkLibraryRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<LinkLibrary>>> GetPaginatedLibraryListAsync(
            BlogFilter filter)
        {
            return await _linkLibraryRepository.GetPaginatedListAsync(filter);
        }

        public async Task<LinkLibrary> CreateLibraryAsync(int currentUserId, LinkLibrary library, int sectionId)
        {
            library.IsNavigation = false;
            library.Name = library.Name?.Trim();
            library.CreatedAt = DateTime.Now;
            library.CreatedBy = currentUserId;

            await _linkLibraryRepository.AddAsync(library);
            await _linkLibraryRepository.SaveAsync();

            var sectLinkLib = new SectionLinkLibrary()
            {
                LinkLibraryId = library.Id,
                SectionId = sectionId
            };
            _linkLibraryRepository.AddSectionLinkLibrary(sectLinkLib);
            await _linkLibraryRepository.SaveAsync();
            return library;
        }

        public async Task<LinkLibrary> UpdateLibraryAsync(LinkLibrary library)
        {
            library.Name = library.Name?.Trim();
            library.Stub = library.Stub?.Trim();

            _linkLibraryRepository.Update(library);
            await _linkLibraryRepository.SaveAsync();
            return library;
        }

        public async Task DeleteLibraryAsync(int id)
        {
            var library = await _linkLibraryRepository.FindAsync(id);

            if (library.IsNavigation)
            {
                throw new OcudaException("Cannot delete navigation link libraries.");
            }
            var sectLinkLib = _linkLibraryRepository.GetSectionLinkLibraryByLibraryId(id);
            _linkLibraryRepository.RemoveSectionLinkLibrary(sectLinkLib);
            _linkLibraryRepository.Remove(id);
            await _linkLibraryRepository.SaveAsync();
        }

        public async Task<List<LinkLibrary>> GetLinkLibrariesBySection(int sectionId)
        {
            var sectLinkLibs = _linkLibraryRepository.GetLinkLibrariesBySectionId(sectionId);
            var linkLibs = new List<LinkLibrary>();
            if (sectLinkLibs != null)
            {
                foreach (var sectLinkLib in sectLinkLibs)
                {
                    var library = await GetLibraryByIdAsync(sectLinkLib.Id);
                    linkLibs.Add(library);
                }
            }
            return linkLibs;
        }
    }
}
