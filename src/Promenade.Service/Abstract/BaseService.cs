using System;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Abstract;

namespace Ocuda.Promenade.Service.Abstract
{
    public abstract class BaseService<TService>
    {
        protected TimeSpan CacheSlidingExpiration { get; set; }

        protected readonly ILogger<TService> _logger;
        protected readonly IDateTimeProvider _dateTimeProvider;

        protected BaseService(ILogger<TService> logger,
            IDateTimeProvider dateTimeProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));

            CacheSlidingExpiration = new TimeSpan(1, 0, 0);
        }
    }
}
