using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Ops
{
    public class TitleClassRepository
        : OpsRepository<OpsContext, TitleClass, int>, ITitleClassRepository
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public TitleClassRepository(Repository<OpsContext> repositoryFacade,
            ILogger<TitleClassRepository> logger,
            IDateTimeProvider dateTimeProvider) : base(repositoryFacade, logger)
        {
            _dateTimeProvider = dateTimeProvider
                 ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        }

        public async Task AddTitleAsync(int userId, int titleClassId, string title)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentNullException(nameof(title)); }

            var titleClass = await DbSet
                .AsNoTracking()
                .Where(_ => _.Id == titleClassId)
                .SingleOrDefaultAsync();

            if (titleClass != null)
            {
                var mappingExists = await _context.TitleClassMappings
                    .AsNoTracking()
                    .Where(_ => _.TitleClassId == titleClassId && _.UserTitle == title)
                    .AnyAsync();

                if (!mappingExists)
                {
                    titleClass.UpdatedBy = userId;
                    titleClass.UpdatedAt = _dateTimeProvider.Now;
                    _context.Update(titleClass);
                    await _context.TitleClassMappings.AddAsync(new TitleClassMapping
                    {
                        TitleClassId = titleClassId,
                        UserTitle = title.Trim()
                    });
                    await _context.SaveChangesAsync();
                }
            }
        }

        public override async Task<TitleClass> FindAsync(int id)
        {
            var item = await base.FindAsync(id);

            item.TitleClassMappings = await _context.TitleClassMappings
                .AsNoTracking()
                .Where(_ => _.TitleClassId == item.Id)
                .OrderBy(_ => _.UserTitle)
                .ToListAsync();

            return item;
        }

        public async Task<IEnumerable<TitleClass>> GetByTitleAsync(string title)
        {
            var ids = _context.TitleClassMappings
                .AsNoTracking()
                .Where(_ => _.UserTitle == title)
                .Select(_ => _.TitleClassId);

            return await DbSet
                .AsNoTracking()
                .Where(_ => ids.Contains(_.Id))
                .ToListAsync();
        }

        public async Task<CollectionWithCount<TitleClass>> GetPaginatedAsync(BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            var data = await query.OrderBy(_ => _.Name).ApplyPagination(filter).ToListAsync();

            foreach (var item in data)
            {
                item.TitleClassMappings = await _context
                    .TitleClassMappings
                    .AsNoTracking()
                    .Where(_ => _.TitleClassId == item.Id)
                    .OrderBy(_ => _.UserTitle)
                    .ToListAsync();
            }

            return new CollectionWithCount<TitleClass>
            {
                Count = await query.CountAsync(),
                Data = data
            };
        }

        public async Task<int> NewTitleClassificationAsync(int userId,
            string titleClassName,
            string title)
        {
            if (string.IsNullOrEmpty(titleClassName))
            {
                throw new ArgumentNullException(nameof(titleClassName));
            }
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            var titleClass = new TitleClass
            {
                CreatedAt = _dateTimeProvider.Now,
                CreatedBy = userId,
                Name = titleClassName,
                TitleClassMappings = new List<TitleClassMapping>
                {
                    new TitleClassMapping
                    {
                        UserTitle = title.Trim()
                    }
                }
            };
            await AddAsync(titleClass);
            await SaveAsync();

            return titleClass.Id;
        }

        public async Task<bool> RemoveTitleAsync(int titleClassId, string title)
        {
            bool deletedTitle = false;
            if (string.IsNullOrEmpty(nameof(title)))
            {
                throw new ArgumentNullException(nameof(title));
            }

            var titleClassMapping = await _context.TitleClassMappings
                .AsNoTracking()
                .Where(_ => _.TitleClassId == titleClassId && _.UserTitle == title.Trim())
                .SingleOrDefaultAsync();

            if (titleClassMapping != null)
            {
                _context.TitleClassMappings.Remove(titleClassMapping);
            }

            var howMany = await _context.TitleClassMappings
                .AsNoTracking()
                .Where(_ => _.TitleClassId == titleClassId)
                .CountAsync();

            if (howMany == 1)
            {
                Remove(titleClassId);
                deletedTitle = true;
            }
            await _context.SaveChangesAsync();
            return deletedTitle;
        }
    }
}