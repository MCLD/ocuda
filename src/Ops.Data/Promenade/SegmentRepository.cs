﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class SegmentRepository : GenericRepository<PromenadeContext, Segment>, ISegmentRepository
    {
        public SegmentRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<Segment> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<Segment>> GetAllActiveSegmentsAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Segment>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Segment>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<int?> GetPageHeaderIdForSegmentAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.SegmentId == id)
                .Select(_ => _.PageLayout.PageHeaderId)
                .SingleOrDefaultAsync();
        }

        public async Task<int?> GetPageLayoutIdForSegmentAsync(int id)
        {
            return await _context.PageItems
                .AsNoTracking()
                .Where(_ => _.SegmentId == id)
                .Select(_ => (int?)_.PageLayoutId)
                .SingleOrDefaultAsync();
        }
    }
}
