using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service.Abstract;
using Ocuda.Promenade.Service.Interfaces.Repositories;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service
{
    public class ExternalResourceService : BaseService<ExternalResourceService>
    {
        private readonly IExternalResourceRepository _externalResourceRepository;

        public ExternalResourceService(ILogger<ExternalResourceService> logger,
            IDateTimeProvider dateTimeProvider,
            IExternalResourceRepository externalResourceRepository)
            : base(logger, dateTimeProvider)
        {
            _externalResourceRepository = externalResourceRepository
                ?? throw new ArgumentNullException(nameof(externalResourceRepository));
        }

        public async Task<ICollection<ExternalResource>> GetAllAsync(ExternalResourceType? type)
        {
            return await _externalResourceRepository.GetAllAsync(type);
        }
    }
}
