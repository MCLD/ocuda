using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Service.Models;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service
{
    public class SocialCardService : ISocialCardService
    {
        private readonly ILogger<SocialCardService> _logger;
        private readonly ISocialCardRepository _socialCardRepository;

        public SocialCardService(ILogger<SocialCardService> logger,
            ISocialCardRepository socialCardRepository)
        {
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
            _socialCardRepository = socialCardRepository
                ?? throw new ArgumentNullException(nameof(socialCardRepository));
        }

        public async Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _socialCardRepository.GetPaginatedListAsync(filter);
        }

        public async Task<SocialCard> GetByIdAsyn(int id)
        {
            return await _socialCardRepository.FindAsync(id);
        }

        public async Task<SocialCard> CreateAsync(SocialCard card)
        {
            card.Description = card.Description?.Trim();
            card.Image = card.Image?.Trim();
            card.ImageAlt = card.ImageAlt?.Trim();
            card.Title = card.Title?.Trim();

            await _socialCardRepository.AddAsync(card);
            await _socialCardRepository.SaveAsync();
            return card;
        }

        public async Task<SocialCard> EditAsync(SocialCard card)
        {
            var currentCard = await _socialCardRepository.FindAsync(card.Id);
            currentCard.Description = card.Description?.Trim();
            currentCard.Image = card.Image?.Trim();
            currentCard.ImageAlt = card.ImageAlt?.Trim();
            currentCard.Title = card.Title?.Trim();

            _socialCardRepository.Update(currentCard);
            await _socialCardRepository.SaveAsync();
            return currentCard;
        }

        public async Task DeleteAsync(int id)
        {
            _socialCardRepository.Remove(id);
            await _socialCardRepository.SaveAsync();
        }
    }
}
