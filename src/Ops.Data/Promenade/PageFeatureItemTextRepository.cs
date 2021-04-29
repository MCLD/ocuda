using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class PageFeatureItemTextRepository :
        GenericRepository<PromenadeContext, PageFeatureItemText>,
        IPageFeatureItemTextRepository
    {
        public PageFeatureItemTextRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<PageFeatureItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public void DetachEntity(PageFeatureItemText itemText)
        {
            _context.Entry(itemText).State = EntityState.Detached;
        }

        public async Task<PageFeatureItemText> GetByPageFeatureItemAndLanguageAsync(
            int pageFeatureItemId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureItemId == pageFeatureItemId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<PageFeatureItemText>> GetAllForPageFeatureItemAsync(
            int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.PageFeatureItemId == itemId)
                .ToListAsync();
        }
    }
}
