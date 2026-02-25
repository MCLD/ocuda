using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class TopicService : BaseService<TopicService>, ITopicService
    {
        private readonly IEmediaTopicRepository _emediaTopicRepository;
        private readonly ITopicRepository _topicRepository;
        private readonly ITopicTextRepository _topicTextRepository;

        public TopicService(ILogger<TopicService> logger,
            IHttpContextAccessor httpContextAccessor,
            ITopicRepository topicRepository,
            ITopicTextRepository topicTextRepository,
            IEmediaTopicRepository emediaTopicRepository)
            : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(emediaTopicRepository);
            ArgumentNullException.ThrowIfNull(topicRepository);
            ArgumentNullException.ThrowIfNull(topicTextRepository);

            _emediaTopicRepository = emediaTopicRepository;
            _topicRepository = topicRepository;
            _topicTextRepository = topicTextRepository;
        }

        public async Task<Topic> CreateAsync(Topic topic)
        {
            ArgumentNullException.ThrowIfNull(topic);
            topic.Name = topic.Name?.Trim();

            await _topicRepository.AddAsync(topic);
            await _topicRepository.SaveAsync();

            return topic;
        }

        public async Task DeleteAsync(int id)
        {
            var topic = await _topicRepository.FindAsync(id)
                ?? throw new OcudaException("Topic does not exist.");

            var topicTexts = await _topicTextRepository.GetAllForTopicAsync(topic.Id);

            var emediaCategories = await _emediaTopicRepository
                .GetByTopicIdAsync(topic.Id);

            _topicTextRepository.RemoveRange(topicTexts);
            _emediaTopicRepository.RemoveRange(emediaCategories);
            _topicRepository.Remove(topic);

            await _topicRepository.SaveAsync();
        }

        public async Task<Topic> EditAsync(Topic topic)
        {
            ArgumentNullException.ThrowIfNull(topic);
            var currentTopic = await _topicRepository.FindAsync(topic.Id);

            currentTopic.Name = topic.Name?.Trim();

            _topicRepository.Update(currentTopic);
            await _topicRepository.SaveAsync();

            return currentTopic;
        }

        public async Task<ICollection<Topic>> GetAllAsync()
        {
            return await _topicRepository.GetAllAsync();
        }

        public async Task<Topic> GetByIdAsync(int id)
        {
            return await _topicRepository.FindAsync(id);
        }

        public async Task<DataWithCount<ICollection<Topic>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _topicRepository.GetPaginatedListAsync(filter);
        }

        public async Task<TopicText> GetTextByTopicAndLanguageAsync(int topicId,
            int languageId)
        {
            return await _topicTextRepository.GetByTopicAndLanguageAsync(topicId,
                languageId);
        }

        public async Task<ICollection<string>> GetTopicEmediasAsync(int id)
        {
            return await _emediaTopicRepository.GetEmediasForTopicAsync(id);
        }

        public async Task<ICollection<string>> GetTopicLanguagesAsync(int id)
        {
            return await _topicTextRepository.GetUsedLanguagesForTopicAsync(id);
        }

        public async Task SetTopicTextAsync(TopicText topicText)
        {
            ArgumentNullException.ThrowIfNull(topicText);

            var currentText = await _topicTextRepository
                .GetByTopicAndLanguageAsync(topicText.TopicId, topicText.LanguageId);

            if (currentText == null)
            {
                topicText.Text = topicText.Text?.Trim();

                await _topicTextRepository.AddAsync(topicText);
            }
            else
            {
                currentText.Text = topicText.Text?.Trim();

                _topicTextRepository.Update(topicText);
            }

            await _topicTextRepository.SaveAsync();
        }
    }
}