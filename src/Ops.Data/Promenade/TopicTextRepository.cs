using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Data.ServiceFacade;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data.Promenade
{
    public class TopicTextRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<TopicTextRepository> logger)
            : GenericRepository<PromenadeContext, TopicText>(repositoryFacade, logger),
            ITopicTextRepository
    {
        public async Task<ICollection<TopicText>> GetAllForTopicAsync(int topicId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.TopicId == topicId)
                .ToListAsync();
        }

        public async Task<TopicText> GetByTopicAndLanguageAsync(int topicId, int languageId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.TopicId == topicId && _.LanguageId == languageId)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<string>> GetUsedLanguagesForTopicAsync(int topicId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.TopicId == topicId)
                .OrderByDescending(_ => _.Language.IsDefault)
                .ThenBy(_ => _.Language.Name)
                .Select(_ => _.Language.Name)
                .ToListAsync();
        }
    }
}