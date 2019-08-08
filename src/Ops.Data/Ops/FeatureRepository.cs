using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Data;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Ops
{
    public class FeatureRepository : GenericRepository<PromenadeContext, Feature, int>, IFeatureRepository
    {
        public FeatureRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LinkRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<List<Feature>> GeAllFeaturesAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<DataWithCount<ICollection<Feature>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            var query = DbSet.AsNoTracking();

            return new DataWithCount<ICollection<Feature>>
            {
                Count = await query.CountAsync(),
                Data = await query
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }


        public async Task<Feature> GetFeatureByName(string featureName)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name.ToLower().Replace(" ","") == featureName)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsDuplicateAsync(Feature feature)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Name == feature.Name
                    || _.Id == feature.Id || (feature.Stub != "" && _.Stub == feature.Stub ))
                .AnyAsync();
        }
    }
}