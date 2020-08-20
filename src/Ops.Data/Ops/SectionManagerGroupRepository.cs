using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;

namespace Ocuda.Ops.Data.Ops
{
    public class SectionManagerGroupRepository
        : OpsRepository<OpsContext, SectionManagerGroup, int>, ISectionManagerGroupRepository
    {
        public SectionManagerGroupRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SectionManagerGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public override async Task<ICollection<SectionManagerGroup>> ToListAsync(
            params Expression<Func<SectionManagerGroup, IComparable>>[] orderBys)
        {
            if (orderBys == null || orderBys.Count() == 0)
            {
                throw new ArgumentNullException(nameof(orderBys));
            }

            return await DbSetOrdered(orderBys)
                .Include(_ => _.Section)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
