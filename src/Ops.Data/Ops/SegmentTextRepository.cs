using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Ops
{
    public class SegmentTextRepository : GenericRepository<PromenadeContext, SegmentText, int>, ISegmentTextRepository
    {
        public SegmentTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentTextRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public SegmentText GetSegmentTextBySegmentId(int segmentId, int languageId)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId && _.LanguageId == languageId)
                .FirstOrDefault();
        }

        public async Task<List<string>> GetLanguageById(int segmentId)
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
