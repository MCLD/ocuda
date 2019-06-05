using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionRepository 
        : GenericRepository<Section, int>, ISectionRepository
    {
        public SectionRepository(OpsContext context, ILogger<SectionRepository> logger)
            : base(context, logger)
        {
        }

        public Task<Section> GetDefaultSectionAsync()
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => string.IsNullOrEmpty(_.Path) && _.IsDeleted == false)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<SectionWithNavigation>> GetNavigationSectionsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .GroupJoin(_context.Links
                                .Where(_ => _.LinkLibrary.IsNavigation)
                                .OrderBy(_ => _.Name),
                      section => section.Id,
                      links => links.LinkLibrary.SectionId,
                      (section, links) => new { section, links })
                .Where(_ => _.section.IsNavigation
                         && _.section.IsDeleted == false)
                .Select(_ => new SectionWithNavigation { Section = _.section, NavigationLinks = _.links })
                .OrderBy(_ => _.Section.SortOrder)
                .ToListAsync();
        }

        public async Task<bool> IsValidPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == path && _.IsDeleted == false)
                .AnyAsync();
        }

        public async Task<Section> GetByNameAsync(string name)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name == name && _.IsDeleted == false)
                .FirstOrDefaultAsync();
        }

        public async Task<Section> GetByPathAsync(string path)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == path && _.IsDeleted == false)
                .FirstOrDefaultAsync();
        }

        public async Task<DataWithCount<ICollection<Section>>> GetPaginatedListAsync
            (BaseFilter filter) {
            var query = DbSet.AsNoTracking().Where(_ => _.IsDeleted == false);

            return new DataWithCount<ICollection<Section>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.SortOrder)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public override async Task<Section> FindAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == id && _.IsDeleted == false)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateNameAsync(Section section)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name == section.Name
                         && _.Id != section.Id
                         && _.IsDeleted == false)
                .AnyAsync();
        }

        public async Task<bool> IsDuplicatePathAsync(Section section)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Path == section.Path
                         && _.Id != section.Id
                         && _.IsDeleted == false)
                .AnyAsync();
        }
    }
}
