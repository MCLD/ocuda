using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service.Interfaces.Ops.Services
{
    public interface IDeckService
    {
        Task AddCardAndDetailAsync(int deckId, int languageId, CardDetail cardDetails);

        Task<int> CardOrderAsync(int cardId, bool increment);

        Task<int> CardsUsingImageAsync(string filename);

        Task<(int cardCount, int cardDetailCount)> CloneAsync(int currentDeckId, int newDeckId);

        Task<Deck> CreateNoSaveAsync(Deck deck);

        Task<(int deckId, int pageLayoutId)> DeleteCardAsync(int cardId);

        Task DeleteDeckNoSaveAsync(int deckId);

        Task EditAsync(Deck deck);

        Task<Deck> GetByIdAsync(int deckId);

        Task<int> GetCardCountAsync(int deckId);

        Task<CardDetail> GetCardDetailsAsync(int cardId, int languageId);

        Task<ICollection<CardDetail>> GetCardDetailsByDeckAsync(int deckId, int languageId);

        Task<int> GetDeckIdAsync(int cardId);

        Task<string> GetFullImageDirectoryPath(string languageName);

        Task<int?> GetPageHeaderIdAsync(int deckId);

        Task<int?> GetPageLayoutIdAsync(int deckId);

        Task<string> GetUploadImageFilePathAsync(string languageName,
                            string filename,
            bool overwriteIfExists);

        Task RemoveCardImageAsync(int cardId);

        Task RemoveCardImageAsync(int cardId, int languageId);

        Task UpdateCardAsync(int cardId, int languageId, CardDetail cardDetail);
    }
}