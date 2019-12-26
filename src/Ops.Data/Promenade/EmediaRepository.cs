using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.Extensions;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class EmediaRepository 
        : GenericRepository<PromenadeContext, Emedia, int>, IEmediaRepository
    {
        public EmediaRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<EmediaRepository> logger) : base(repositoryFacade, logger)
        {
        }
        public async Task<ICollection<Emedia>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderBy(_ => _.Name)
                .ToListAsync();
        }

        public Emedia GetByClass(string emediaStub)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.Stub == emediaStub)
                .FirstOrDefault();
        }

        public async Task<DataWithCount<ICollection<Emedia>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return new DataWithCount<ICollection<Emedia>>
            {
                Count = await DbSet.AsNoTracking().CountAsync(),
                Data = await DbSet.AsNoTracking()
                    .OrderBy(_ => _.Name)
                    .ApplyPagination(filter)
                    .ToListAsync()
            };
        }
    }
}
