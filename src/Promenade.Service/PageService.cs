using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class PageService : BaseService<PageService>
    {
        private readonly IPageRepository _pageRepository;

        public PageService(ILogger<PageService> logger,
            IDateTimeProvider dateTimeProvider,
            IPageRepository pageRepository)
            : base(logger, dateTimeProvider)
        {
            _pageRepository = pageRepository
                ?? throw new ArgumentNullException(nameof(pageRepository));
        }

        public async Task<Page> GetByStubAndType(string stub, PageType type)
        {
            return await _pageRepository.GetByStubAndTypeAsync(stub?.Trim(), type);
        }
    }
}
