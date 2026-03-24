using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaRepository> logger)
            : GenericRepository<PromenadeContext, Emedia>(repositoryFacade, logger),
            IEmediaRepository
    {
        public async Task ApplySlugAsync(int id, string slug)
        {
            ArgumentNullException.ThrowIfNull(slug);
            var revisedSlug = await GetUnusedSlugAsync(slug);
            var item = await DbSet.SingleAsync(_ => _.IsActive && _.Id == id);
            _logger.LogInformation("Updating slug for emedia {Name} ({Id}) to {Slug}",
                item.Name,
                item.Id,
                revisedSlug);
            item.Slug = revisedSlug;
            Update(item);
            await SaveAsync();
        }

        public async Task DeactivateAsync(int emediaId)
        {
            var emedia = await DbSet.SingleOrDefaultAsync(_ => _.IsActive && _.Id == emediaId)
                ?? throw new OcudaException($"Unable to find Emedia with id {emediaId}");
            emedia.IsActive = false;
            DbSet.Update(emedia);
            await _context.SaveChangesAsync();
        }

        public async Task<Emedia> FindAsync(string name, string link)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.Name == name && _.RedirectUrl == link)
                .FirstOrDefaultAsync();
        }

        public async Task<Emedia> FindAsync(string slug)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.IsActive && _.Slug == slug);
        }

        public async Task<Emedia> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.IsActive && _.Id == id);
        }

        public async Task<Emedia> GetIncludingGroupAsync(int id)
        {
            return await DbSet
                .Include(_ => _.Group)
                .Where(_ => _.IsActive && _.Id == id)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IDictionary<int, string>> GetMissingSlugsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.Slug.Length == 0)
                .ToDictionaryAsync(k => k.Id, v => v.Name);
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListForGroupAsync(
            int groupId, BaseFilter filter)
        {
            var query = DbSet.AsNoTracking().Where(_ => _.IsActive && _.GroupId == groupId);

            return new DataWithCount<ICollection<Emedia>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<string> GetUnusedSlugAsync(string slug)
        {
            ArgumentNullException.ThrowIfNull(slug);
            var revisedSlug = slug.Trim().Length > 255 ? slug.Trim()[..255] : slug.Trim();
            var slugInUse = await DbSet
                .AsNoTracking()
                .AnyAsync(_ => _.IsActive && _.Slug == revisedSlug);
            if (slugInUse)
            {
                int count = 1;
                var baseSlug = revisedSlug.Length > 252 ? revisedSlug[..252] : revisedSlug;
                while (slugInUse)
                {
                    revisedSlug = $"{baseSlug.Trim()}-{count++}";
                    slugInUse = await DbSet
                        .AsNoTracking()
                        .AnyAsync(_ => _.IsActive && _.Slug == revisedSlug);
                }
                if (count > 100)
                {
                    throw new Utility.Exceptions.OcudaException("Unable to create a unique slug.");
                }
            }
            return revisedSlug;
        }

        public override void Remove(Emedia entity)
        {
            throw new InvalidOperationException();
        }

        public override void RemoveRange(ICollection<Emedia> entities)
        {
            throw new InvalidOperationException();
        }
    }
}