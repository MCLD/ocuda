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
    public class EmediaTopicRepository(Repository<PromenadeContext> repositoryFacade,
        ILogger<EmediaTopicRepository> logger)
            : GenericRepository<PromenadeContext, EmediaTopic>(repositoryFacade, logger),
            IEmediaTopicRepository
    {
        public async Task<ICollection<EmediaTopic>> GetAllForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaTopic>> GetAllForGroupAsync(int groupId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.Emedia.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<ICollection<EmediaTopic>> GetByTopicIdAsync(int topicId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.TopicId == topicId)
                .ToListAsync();
        }

        public async Task<ICollection<string>> GetEmediasForTopicAsync(int topicId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.TopicId == topicId)
                .Select(_ => _.Emedia.Name)
                .OrderBy(_ => _)
                .ToListAsync();
        }

        public async Task<ICollection<int>> GetTopicIdsForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId)
                .Select(_ => _.TopicId)
                .ToListAsync();
        }

        public async Task<ICollection<Topic>> GetTopicsForEmediaAsync(int emediaId)
        {
            return await DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId)
                .Select(_ => _.Topic)
                .ToListAsync();
        }

        public void RemoveByEmediaAndTopics(int emediaId, ICollection<int> topicIds)
        {
            var emediaTopics = DbSet
                .AsNoTracking()
                .Where(_ => _.EmediaId == emediaId && topicIds.Contains(_.TopicId));

            DbSet.RemoveRange(emediaTopics);
        }
    }
}