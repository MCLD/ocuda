using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Repositories
{
    public interface ISectionManagerGroupRepository : IRepository<SectionManagerGroup, int>
    {
        new Task<ICollection<SectionManagerGroup>>
            ToListAsync(params Expression<Func<SectionManagerGroup, IComparable>>[] orderBys);
    }
}
