using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Data.Promenade
{
    public class SubjectRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<SubjectRepository> logger)
            : GenericRepository<PromenadeContext, Subject>(repositoryFacade, logger),
            ISubjectRepository
    {
        public async Task<Subject> FindAsync(int id)
        {
            var entity = await DbSet.FindAsync(id);
            if (entity != null)
            {
                _context.Entry(entity).State = EntityState.Detached;
            }
            return entity;
        }

        public async Task<ICollection<Subject>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Subject>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Subject>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }

        public async Task<string> GetUnusedSlugAsync(string slug)
        {
            ArgumentNullException.ThrowIfNull(slug);
            var revisedSlug = slug.Trim().Length > 255 ? slug.Trim()[..255] : slug.Trim();
            var slugInUse = await DbSet.AsNoTracking().AnyAsync(_ => _.Slug == revisedSlug);
            if (slugInUse)
            {
                int count = 1;
                var baseSlug = revisedSlug.Length > 252 ? revisedSlug[..252] : revisedSlug;
                while (slugInUse)
                {
                    revisedSlug = $"{baseSlug.Trim()}-{count++}";
                    slugInUse = await DbSet.AsNoTracking().AnyAsync(_ => _.Slug == revisedSlug);
                }
                if (count > 100)
                {
                    throw new Utility.Exceptions.OcudaException("Unable to create a unique slug.");
                }
            }
            return revisedSlug;
        }
    }
}