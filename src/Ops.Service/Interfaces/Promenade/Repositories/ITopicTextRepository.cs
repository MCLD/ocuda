using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Repositories
{
    public interface ITopicTextRepository : IGenericRepository<TopicText>
    {
        Task<ICollection<TopicText>> GetAllForTopicAsync(int topicId);

        Task<TopicText> GetByTopicAndLanguageAsync(int topicId, int languageId);

        Task<ICollection<string>> GetUsedLanguagesForTopicAsync(int topicId);
    }
}