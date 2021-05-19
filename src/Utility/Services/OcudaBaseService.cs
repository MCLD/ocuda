using System;
using Microsoft.Extensions.Logging;

namespace Ocuda.Utility.Services
{
    public abstract class OcudaBaseService<TService>
    {
        protected readonly ILogger<TService> _logger;

        protected OcudaBaseService(ILogger<TService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}