using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service.Abstract
{
    public abstract class BaseService<Service>
    {
        protected readonly ILogger<Service> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public BaseService(ILogger<Service> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected int GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userIdClaim = httpContext.User.Claims.Where(_ => _.Type == ClaimType.UserId).First();

            if (int.TryParse(userIdClaim.Value, out int id))
            {
                return id;
            }
            else
            {
                throw new Exception($"Could not convert '{ClaimType.UserId}' to a number.");
            }
        }
    }
}
