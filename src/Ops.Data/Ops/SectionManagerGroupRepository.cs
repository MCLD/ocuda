using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        : GenericRepository<OpsContext, SectionManagerGroup, int>, ISectionManagerGroupRepository
    {
        public SectionManagerGroupRepository(ServiceFacade.Repository<OpsContext> repositoryFacade,
            ILogger<SectionManagerGroupRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public override async Task<ICollection<SectionManagerGroup>> ToListAsync(
            params Expression<Func<SectionManagerGroup, IComparable>>[] orderBys)
        {
            Contract.Requires(orderBys != null && orderBys.Any());

            return await DbSetOrdered(orderBys)
                .Include(_ => _.Section)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
