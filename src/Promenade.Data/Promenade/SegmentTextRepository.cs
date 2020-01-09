using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Interfaces.Repositories;

namespace Ocuda.Promenade.Data.Promenade
{
    public class SegmentTextRepository
    : GenericRepository<PromenadeContext, SegmentText, int>, ISegmentTextRepository
    {
        public SegmentTextRepository(ServiceFacade.Repository<PromenadeContext> repositoryFacade,
            ILogger<SegmentRepository> logger) : base(repositoryFacade, logger)
        {
        }

        public SegmentText GetSegmentTextBySegmentAndLanguageId(int segmentId, int languageId)
        {
            return DbSet
                .AsNoTracking()
                .Where(_ => _.SegmentId == segmentId && _.LanguageId == languageId)
                .FirstOrDefault();
        }
    }
}