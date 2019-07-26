using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class SocialCardService : BaseService<SocialCardService>
    {
        private readonly ISocialCardRepository _socialCardRepository;
        private readonly SiteSettingService _siteSettingService;

        public SocialCardService(ILogger<SocialCardService> logger,
            IDateTimeProvider dateTimeProvider,
            ISocialCardRepository socialCardRepository,
            SiteSettingService siteSettingService)
            : base(logger, dateTimeProvider)
        {
            _socialCardRepository = socialCardRepository
                ?? throw new ArgumentNullException(nameof(socialCardRepository));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task<SocialCard> GetByIdAsync(int id)
        {
            var card = await _socialCardRepository.FindAsync(id);

            card.TwitterSite = await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.Social.TwitterSite);

            return card;
        }
    }
}
