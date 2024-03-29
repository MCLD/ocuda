﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaCategoryRepository
        : GenericRepository<PromenadeContext, EmediaCategory>, IEmediaCategoryRepository
    {
        public EmediaCategoryRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaCategoryRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<Category>> GetCategoriesForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId)
                .Select(_ => _.Category)
                .ToListAsync();
        }

        public async Task<ICollection<int>> GetCategoryIdsForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId)
                .Select(_ => _.CategoryId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaCategory>> GetByCategoryIdAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaCategory>> GetAllForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaCategory>> GetAllForGroupAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<ICollection<string>> GetEmediasForCategoryAsync(int categoryId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.CategoryId == categoryId)
                .Select(_ => _.Emedia.Name)
                .OrderBy(_ => _)
                .ToListAsync();
        }

        public void RemoveByEmediaAndCategories(int emediaId, ICollection<int> categoryIds)
        {
            var emediaCategories = DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId && categoryIds.Contains(_.CategoryId));

            DbSet.RemoveRange(emediaCategories);
        }
    }
}
