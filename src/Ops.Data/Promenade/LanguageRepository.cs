using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class LanguageRepository
        : GenericRepository<PromenadeContext, Language>, ILanguageRepository
    {
        public LanguageRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<LanguageRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<Language>> GetAllAsync()
        {
            return await DbSet
                .AsNoTracking()
                .OrderByDescending(_ => _.IsDefault)
                .ThenBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<ICollection<Language>> GetActiveAsync()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive)
                .OrderByDescending(_ => _.IsDefault)
                .ThenBy(_ => _.Name)
                .ToListAsync();
        }

        public async Task<Language> GetActiveByIdAsync(int id)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.Id == id)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetDefaultLanguageId()
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.IsDefault)
                .Select(_ => _.Id)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetLanguageId(string culture)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.IsActive && _.Name == culture)
                .Select(_ => _.Id)
                .SingleOrDefaultAsync();
        }
    }
}
