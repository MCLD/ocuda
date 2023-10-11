using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class PageHeaderRepository : GenericRepository<PromenadeContext, PageHeader>,
        IPageHeaderRepository
    {
        public PageHeaderRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageHeaderRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<PageHeader> GetByIdAndTypeAsync(int id, PageType type)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Id == id && _.Type == type);
        }

        public async Task<PageHeader> GetByStubAndTypeAsync(string stub, PageType type)
        {
            return await DbSet
                .AsNoTracking()
                .SingleOrDefaultAsync(_ => _.Stub == stub && _.Type == type);
        }
    }
}