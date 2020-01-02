using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service.Abstract
{
    public abstract class BaseService<TService>
    {
        protected readonly ILogger<TService> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(ILogger<TService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected int GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userIdClaim = httpContext.User.Claims.First(_ => _.Type == ClaimType.UserId);

            if (int.TryParse(userIdClaim.Value, out int id))
            {
                return id;
            }
            else
            {
                throw new OcudaException($"Could not convert '{ClaimType.UserId}' to a number.");
            }
        }
    }
}
