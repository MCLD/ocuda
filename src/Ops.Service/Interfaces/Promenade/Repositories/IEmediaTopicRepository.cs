using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface IEmediaTopicRepository : IGenericRepository<EmediaTopic>
    {
        Task<ICollection<EmediaTopic>> GetAllForEmediaAsync(int emediaId);

        Task<ICollection<EmediaTopic>> GetAllForGroupAsync(int groupId);

        Task<ICollection<EmediaTopic>> GetByTopicIdAsync(int topicId);

        Task<ICollection<string>> GetEmediasForTopicAsync(int topicId);

        Task<ICollection<int>> GetTopicIdsForEmediaAsync(int emediaId);

        Task<ICollection<Topic>> GetTopicsForEmediaAsync(int emediaId);

        void RemoveByEmediaAndTopics(int emediaId, ICollection<int> topicIds);
    }
}