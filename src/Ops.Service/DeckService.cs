using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class DeckService : BaseService<DeckService>, IDeckService
    {
        private const string CardsFilePath = "cards";

        private readonly ICardDetailRepository _cardDetailRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IDeckRepository _deckRepository;
        private readonly ILanguageService _languageService;
        private readonly IPageItemRepository _pageItemRepository;
        private readonly ISiteSettingService _siteSettingService;

        public DeckService(ILogger<DeckService> logger,
            IHttpContextAccessor httpContextAccessor,
            IDeckRepository deckRepository,
            ICardDetailRepository cardDetailRepository,
            ICardRepository cardRepository,
            ILanguageService languageService,
            IPageItemRepository pageItemRepository,
            ISiteSettingService siteSettingService) : base(logger, httpContextAccessor)
        {
            _deckRepository = deckRepository
                ?? throw new ArgumentNullException(nameof(deckRepository));
            _cardDetailRepository = cardDetailRepository
                ?? throw new ArgumentNullException(nameof(cardDetailRepository));
            _cardRepository = cardRepository
                ?? throw new ArgumentNullException(nameof(cardRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _pageItemRepository = pageItemRepository
                ?? throw new ArgumentNullException(nameof(pageItemRepository));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task AddCardAndDetailAsync(int deckId,
            int languageId,
            CardDetail cardDetails)
        {
            if (cardDetails == null)
            {
                throw new ArgumentNullException(nameof(cardDetails));
            }

            cardDetails.Card = await _cardRepository.AddWithOrderAsync(new Card
            {
                DeckId = deckId
            });
            cardDetails.LanguageId = languageId;
            cardDetails.AltText = cardDetails.AltText?.Trim();
            cardDetails.Filename = cardDetails.Filename?.Trim();
            cardDetails.Header = cardDetails.Header?.Trim();
            cardDetails.Link = cardDetails.Link?.Trim();
            cardDetails.Text = cardDetails.Text?.Trim();
            cardDetails.Footer = cardDetails.Footer?.Trim();

            await _cardDetailRepository.AddAsync(cardDetails);
            await _cardDetailRepository.SaveAsync();
        }

        public async Task<int> CardOrderAsync(int cardId, bool increment)
        {
            var cards = await _cardRepository.GetOrderInformationById(cardId);

            var requestedCard = cards.SingleOrDefault(_ => _.Id == cardId)
                ?? throw new OcudaException($"Unable to find card id {cardId}");

            if (increment)
            {
                var nextCard = cards.SingleOrDefault(_ => _.Order == requestedCard.Order + 1)
                    ?? throw new OcudaException($"Card id {cardId} is already the last card.");
                nextCard.Order--;
                requestedCard.Order++;
                _cardRepository.Update(nextCard);
            }
            else
            {
                var prevCard = cards.SingleOrDefault(_ => _.Order == requestedCard.Order - 1);
                if (prevCard == null || requestedCard.Order <= 1)
                {
                    throw new OcudaException($"Card id {cardId} is already the first card.");
                }
                prevCard.Order++;
                requestedCard.Order--;
                _cardRepository.Update(prevCard);
            }

            _cardRepository.Update(requestedCard);
            await _cardRepository.SaveAsync();

            return requestedCard.DeckId;
        }

        public async Task<int> CardsUsingImageAsync(string filename)
        {
            return await _cardDetailRepository.CardsUsingImageAsync(filename);
        }

        public async Task<(int cardCount, int cardDetailCount)> CloneAsync(int currentDeckId, int newDeckId)
        {
            int cardCount = 0;
            int cardDetailCount = 0;
            foreach (var card in await _cardRepository.GetByDeckAsync(currentDeckId))
            {
                cardCount++;
                var newCard = new Card
                {
                    DeckId = newDeckId,
                    Order = card.Order
                };
                await _cardRepository.AddAsync(newCard);
                await _cardRepository.SaveAsync();

                foreach (var cardDetail in await _cardDetailRepository.GetByCardId(card.Id))
                {
                    cardDetailCount++;
                    await _cardDetailRepository.AddAsync(new CardDetail
                    {
                        AltText = cardDetail.AltText,
                        CardId = newCard.Id,
                        Filename = cardDetail.Filename,
                        Header = cardDetail.Header,
                        LanguageId = cardDetail.LanguageId,
                        Link = cardDetail.Link,
                        Text = cardDetail.Text
                    });
                }
                await _cardDetailRepository.SaveAsync();
            }

            return (cardCount, cardDetailCount);
        }

        public async Task<Deck> CreateNoSaveAsync(Deck deck)
        {
            await _deckRepository.AddAsync(deck);
            return deck;
        }

        public async Task<(int deckId, int pageLayoutId)> DeleteCardAsync(int cardId)
        {
            await RemoveCardImageAsync(cardId);
            await _cardDetailRepository.DeleteCardDetails(cardId);
            var deckId = await _cardRepository.DeleteCard(cardId);

            var cards = await GetCardCountAsync(deckId);

            if (cards == 0)
            {
                int layoutId = await _pageItemRepository.RemoveByDeckIdAsync(deckId);
                await _deckRepository.DeleteDeckAsync(deckId);
                return (0, layoutId);
            }
            else
            {
                await _deckRepository.FixOrderAsync(deckId);
            }

            return (deckId, 0);
        }

        public async Task DeleteDeckNoSaveAsync(int deckId)
        {
            var cards = await _cardRepository.GetByDeckAsync(deckId);
            IEnumerable<CardDetail> allDetails = new List<CardDetail>();
            foreach (var card in cards)
            {
                await RemoveCardImageAsync(card.Id);
                var details = await _cardDetailRepository.GetByCardId(card.Id);
                allDetails = allDetails.Union(details);
            }
            _cardDetailRepository.RemoveRange(allDetails.ToList());
            _cardRepository.RemoveRange(cards);
            _deckRepository.Remove(await _deckRepository.GetByIdAsync(deckId));
        }

        public async Task EditAsync(Deck deck)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            var updateDeck = await _deckRepository.GetByIdAsync(deck.Id);
            if (updateDeck != null)
            {
                updateDeck.Name = deck.Name;
            }
            _deckRepository.Update(updateDeck);
            await _deckRepository.SaveAsync();
        }

        public async Task<Deck> GetByIdAsync(int deckId)
        {
            return await _deckRepository.GetByIdAsync(deckId);
        }

        public async Task<int> GetCardCountAsync(int deckId)
        {
            return await _cardRepository.GetCountByDeckIdAsync(deckId);
        }

        public async Task<CardDetail> GetCardDetailsAsync(int cardId, int languageId)
        {
            var cardDetail = await _cardDetailRepository.GetByIds(cardId, languageId);
            if (cardDetail != null && !string.IsNullOrEmpty(cardDetail?.Filename))
            {
                var language = await _languageService.GetActiveByIdAsync(languageId);
                var imageBasePath = await GetFullImageDirectoryPath(_siteSettingService,
                    language.Name,
                    CardsFilePath);

                cardDetail.ImagePath = Path.Combine(imageBasePath, cardDetail.Filename);
            }
            return cardDetail;
        }

        public async Task<ICollection<CardDetail>> GetCardDetailsByDeckAsync(int deckId, int languageId)
        {
            return await _cardDetailRepository.GetByDeckLanguageAsync(deckId, languageId);
        }

        public async Task<int> GetDeckIdAsync(int cardId)
        {
            return await _cardRepository.GetDeckIdAsync(cardId);
        }

        public async Task<string> GetFullImageDirectoryPath(string languageName)
        {
            return await GetFullImageDirectoryPath(_siteSettingService,
                    languageName,
                    CardsFilePath);
        }

        public async Task<int?> GetPageHeaderIdAsync(int deckId)
        {
            return await _deckRepository.GetPageHeaderIdAsync(deckId);
        }

        public async Task<int?> GetPageLayoutIdAsync(int deckId)
        {
            return await _deckRepository.GetPageLayoutIdAsync(deckId);
        }

        public async Task<string> GetUploadImageFilePathAsync(string languageName,
            string filename,
            bool overwriteIfExists)
        {
            var cardPath = await GetFullImageDirectoryPath(_siteSettingService,
                languageName,
                CardsFilePath);
            var fullFilePath = Path.Combine(cardPath, filename);

            if (!overwriteIfExists)
            {
                int renameCounter = 1;
                while (File.Exists(fullFilePath))
                {
                    fullFilePath = Path.Combine(cardPath, string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}-{1}{2}",
                        Path.GetFileNameWithoutExtension(filename),
                        renameCounter++,
                        Path.GetExtension(filename)));
                }
            }

            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }

            return fullFilePath;
        }

        public Task RemoveCardImageAsync(int cardId)
        {
            return RemoveCardImageInternalAsync(cardId, null);
        }

        public Task RemoveCardImageAsync(int cardId, int languageId)
        {
            return RemoveCardImageInternalAsync(cardId, languageId);
        }

        public async Task UpdateCardAsync(int cardId, int languageId, CardDetail cardDetail)
        {
            if (cardDetail == null)
            {
                throw new ArgumentNullException(nameof(cardDetail));
            }

            var updateDetail = await _cardDetailRepository.GetByIds(cardId, languageId);

            var isNew = updateDetail == null;

            updateDetail ??= new CardDetail
            {
                CardId = cardId,
            };

            updateDetail.LanguageId = languageId;
            updateDetail.AltText = cardDetail.AltText?.Trim();
            updateDetail.Filename = cardDetail.Filename?.Trim();
            updateDetail.Header = cardDetail.Header?.Trim();
            updateDetail.Link = cardDetail.Link?.Trim();
            updateDetail.Text = cardDetail.Text?.Trim();
            updateDetail.Footer = cardDetail.Footer?.Trim();

            if (isNew)
            {
                await _cardDetailRepository.AddAsync(updateDetail);
            }
            else
            {
                _cardDetailRepository.Update(updateDetail);
            }

            await _cardDetailRepository.SaveAsync();
        }

        private async Task RemoveCardImageInternalAsync(int cardId, int? languageId)
        {
            ICollection<CardDetail> cards;
            var languages = await _languageService.GetActiveAsync();
            if (languageId.HasValue)
            {
                cards = new List<CardDetail> {
                   await _cardDetailRepository.GetByIds(cardId, languageId.Value)
                };
            }
            else
            {
                cards = await _cardDetailRepository.GetByCardId(cardId);
            }
            foreach (var card in cards)
            {
                if (!string.IsNullOrEmpty(card.Filename))
                {
                    var languageName = languages
                        .Where(_ => _.Id == card.LanguageId)
                        .Select(_ => _.Name)
                        .SingleOrDefault();
                    if (languageName == null)
                    {
                        _logger.LogError("Unable to find language {LanguageId} even though it is associated with card {CardId}",
                            languageId,
                            cardId);
                    }
                    else
                    {
                        // check if this card is in use elsewhere
                        var imageUseCount = await _cardDetailRepository
                            .CardsUsingImageAsync(card.Filename);

                        var baseImagePath = await GetFullImageDirectoryPath(_siteSettingService,
                            languageName,
                            CardsFilePath);

                        if (imageUseCount == 1)
                        {
                            File.Delete(Path.Combine(baseImagePath, card.Filename));
                            _logger.LogInformation("Card image {Filename} deleted, only used in 1 card.",
                                card.Filename);
                        }
                        else
                        {
                            _logger.LogInformation("Card image {Filename} not deleted, used in {UseCount} cards",
                                card.Filename,
                                imageUseCount);
                        }
                    }
                }
            }
        }
    }
}