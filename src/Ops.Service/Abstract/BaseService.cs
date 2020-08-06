using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Service.Abstract
{
    public abstract class BaseService<TService> : Utility.Services.OcudaBaseService<TService>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseService(ILogger<TService> logger,
            IHttpContextAccessor httpContextAccessor)
            : base(logger)
        {
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext
                .User
                .Claims
                .First(_ => _.Type == ClaimType.UserId);

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
