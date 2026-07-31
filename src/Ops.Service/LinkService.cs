using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class LinkService(
        ILogger<LinkService> logger,
        IHttpContextAccessor httpContextAccessor,
        IDateTimeProvider dateTimeProvider,
        ILinkLibraryRepository linkLibraryRepository,
        ILinkRepository linkRepository)
        : BaseService<LinkService>(logger, httpContextAccessor),
        ILinkService
    {
        public async Task<Link> CreateAsync(Link link)
        {
            ArgumentNullException.ThrowIfNull(link);

            var newLink = new Link
            {
                Name = link.Name?.Trim(),
                Url = link.Url?.Trim(),
                Icon = link.Icon?.Trim(),
                LinkLibraryId = link.LinkLibraryId,
                CreatedAt = DateTime.Now,
                CreatedBy = GetCurrentUserId(),
            };

            await linkRepository.AddAsync(newLink);
            await linkRepository.SaveAsync();
            return link;
        }

        public async Task DeleteAsync(int id)
        {
            linkRepository.Remove(id);
            await linkRepository.SaveAsync();
        }

        public async Task<LinkLibrary> CreateLibraryAsync(LinkLibrary library)
        {
            ArgumentNullException.ThrowIfNull(library);

            library.Name = library.Name?.Trim();
            library.Slug = library.Slug?.Trim();

            var exists = await linkLibraryRepository
                .GetBySectionIdSlugAsync(library.SectionId, library.Slug);

            if (exists != null)
            {
                throw new OcudaException(
                    $"A link library for this section already exists with this slug: {library.Slug}");
            }

            library.CreatedAt = dateTimeProvider.Now;
            library.CreatedBy = GetCurrentUserId();
            library.IsNavigation = false;

            await linkLibraryRepository.AddAsync(library);
            await linkLibraryRepository.SaveAsync();

            return library;
        }

        public async Task DeleteLibraryAsync(int id)
        {
            var library = await linkLibraryRepository.FindAsync(id);

            if (library.IsNavigation)
            {
                throw new OcudaException("Cannot delete navigation link libraries.");
            }

            linkLibraryRepository.Remove(id);
            await linkLibraryRepository.SaveAsync();
        }

        public async Task<Link> EditAsync(Link link)
        {
            var currentLink = await linkRepository.FindAsync(link.Id);

            currentLink.Name = link.Name?.Trim();
            currentLink.Url = link.Url?.Trim();
            currentLink.Icon = link.Icon;
            currentLink.UpdatedAt = dateTimeProvider.Now;
            currentLink.UpdatedBy = GetCurrentUserId();

            linkRepository.Update(currentLink);
            await linkRepository.SaveAsync();
            return link;
        }

        public async Task<Link> GetByIdAsync(int id)
        {
            return await linkRepository.FindAsync(id);
        }

        public async Task<ICollection<LinkLibrary>> GetBySectionIdAsync(int sectionId)
        {
            return await GetBySectionIdAsync(sectionId, null);
        }

        public async Task<LinkLibrary> GetBySectionIdSlugAsync(int sectionId, string slug)
        {
            return await linkLibraryRepository.GetBySectionIdSlugAsync(sectionId, slug);
        }

        public async Task<LinkLibrary> GetLibraryByIdAsync(int id)
        {
            return await linkLibraryRepository.FindAsync(id);
        }

        public async Task<int> GetLinkCountAsync(int linkLibraryId)
        {
            var result = await linkRepository.GetPaginatedListAsync(new LinksFilter
            {
                LinkLibrary = new LinkLibrary
                {
                    Id = linkLibraryId,
                },
                OnlyCount = true,
            });

            return result.Count;
        }

        public async Task<ICollection<Link>> GetLinkLibraryLinksAsync(int id)
        {
            return await linkRepository.GetLinkLibraryLinksAsync(id);
        }

        public async Task<DataWithCount<ICollection<Link>>> GetPaginatedListAsync(
            LinksFilter filter)
        {
            return await linkRepository.GetPaginatedListAsync(filter);
        }

        public async Task UpdateLibrary(Section section, string slug, LinkLibrary library)
        {
            ArgumentNullException.ThrowIfNull(section);
            ArgumentNullException.ThrowIfNull(library);

            var currentLibrary = await GetBySectionIdSlugAsync(section.Id, slug);

            if (currentLibrary.Slug != library.Slug.Trim())
            {
                var checkNewSlug = await GetBySectionIdSlugAsync(section.Id, library.Slug.Trim());

                if (checkNewSlug != null)
                {
                    throw new OcudaException(
                        $"The slug {library.Slug.Trim()} is already in use by link library {checkNewSlug.Name}");
                }

                currentLibrary.Slug = library.Slug.Trim();
            }

            currentLibrary.IsFeatured = library.IsFeatured;
            currentLibrary.Name = library.Name.Trim();
            currentLibrary.UpdatedAt = DateTime.Now;
            currentLibrary.UpdatedBy = GetCurrentUserId();

            linkLibraryRepository.Update(currentLibrary);
            await linkLibraryRepository.SaveAsync();
        }

        private async Task<ICollection<LinkLibrary>> GetBySectionIdAsync(
            int sectionId,
            bool? isFeatured)
        {
            return await linkLibraryRepository.GetBySectionAsync(sectionId, isFeatured);
        }
    }
}
