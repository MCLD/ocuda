using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class SegmentTextRepository 
        : GenericRepository<PromenadeContext, SegmentText>, ISegmentTextRepository
    {
        public SegmentTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public async Task<ICollection<SegmentText>> GetBySegmentIdAsync(int segmentId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId)
                .ToListAsync();
        }

        public async Task<SegmentText> GetBySegmentAndLanguageAsync(int segmentId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetUsedLanguageNamesBySegmentId(int segmentId)
        {
            return await _context.SegmentTexts
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }
    }
}
