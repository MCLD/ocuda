using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Service
{
    public class LanguageService : ILanguageService
    {
        private readonly ILanguageRepository _languageRepository;

        public LanguageService(ILanguageRepository languageRepository)
        {
            _languageRepository = languageRepository
                ?? throw new ArgumentNullException(nameof(languageRepository));
        }

        public async Task<ICollection<Language>> GetActiveAsync()
        {
            return await _languageRepository.GetActiveAsync();
        }

        public async Task<Language> GetActiveByIdAsync(int id)
        {
            return await _languageRepository.GetActiveByIdAsync(id);
        }

        public async Task<IDictionary<int, string>> GetActiveNamesAsync()
        {
            return await _languageRepository.GetActiveNamesAsync();
        }

        public async Task<int> GetDefaultLanguageId()
        {
            return await _languageRepository.GetDefaultLanguageId();
        }
    }
}