using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaCategoryRepository : GenericRepository<PromenadeContext, EmediaCategory, int>, IEmediaCategoryRepository
    {
        public EmediaCategoryRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaCategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<EmediaCategory>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ICollection<EmediaCategory>> GetByEmediaIdAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_=>_.EmediaId == emediaId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaCategory>> GetByCategoryIdAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .ToListAsync();
        }

        public EmediaCategory GetByEmediaAndCategoryId
            (int emediaId, int categoryId)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId && _.CategoryId == categoryId)
                .FirstOrDefault();
        }
    }
}
