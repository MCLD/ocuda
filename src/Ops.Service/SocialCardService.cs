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
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class SocialCardService : BaseService<SocialCardService>, ISocialCardService
    {
        private readonly ISocialCardRepository _socialCardRepository;

        public SocialCardService(ILogger<SocialCardService> logger,
            IHttpContextAccessor httpContextAccessor,
            ISocialCardRepository socialCardRepository)
            : base(logger, httpContextAccessor)
        {
            _socialCardRepository = socialCardRepository
                ?? throw new ArgumentNullException(nameof(socialCardRepository));
        }

        public async Task<ICollection<SocialCard>> GetListAsync()
        {
            return await _socialCardRepository.ToListAsync(_ => _.Title);
        }

        public async Task<DataWithCount<ICollection<SocialCard>>> GetPaginatedListAsync(
            BaseFilter filter)
        {
            return await _socialCardRepository.GetPaginatedListAsync(filter);
        }

        public async Task<SocialCard> GetByIdAsync(int id)
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
            var socialCard = await _socialCardRepository.FindAsync(id);
            _socialCardRepository.Remove(socialCard);
            await _socialCardRepository.SaveAsync();
        }
    }
}
