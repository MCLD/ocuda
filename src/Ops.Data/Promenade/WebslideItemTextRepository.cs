using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class WebslideItemTextRepository : GenericRepository<PromenadeContext, WebslideItemText>,
        IWebslideItemTextRepository
    {
        public WebslideItemTextRepository(
            ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<WebslideItemTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public void DetachEntity(WebslideItemText itemText)
        {
            _context.Entry(itemText).State = EntityState.Detached;
        }

        public async Task<WebslideItemText> GetByWebslideItemAndLanguageAsync(int webslideItemId,
            int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideItemId == webslideItemId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<WebslideItemText>> GetAllForWebslideItemAsync(int itemId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.WebslideItemId == itemId)
                .ToListAsync();
        }
    }
}
