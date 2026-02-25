using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Filters;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service.Interfaces.Promenade.Services
{
    public interface ITopicService
    {
        Task<Topic> CreateAsync(Topic topic);

        Task DeleteAsync(int id);

        Task<Topic> EditAsync(Topic topic);

        Task<ICollection<Topic>> GetAllAsync();

        Task<Topic> GetByIdAsync(int id);

        Task<DataWithCount<ICollection<Topic>>> GetPaginatedListAsync(BaseFilter filter);

        Task<TopicText> GetTextByTopicAndLanguageAsync(int topicId, int languageId);

        Task<ICollection<string>> GetTopicEmediasAsync(int id);

        Task<ICollection<string>> GetTopicLanguagesAsync(int id);

        Task SetTopicTextAsync(TopicText topicText);
    }
}