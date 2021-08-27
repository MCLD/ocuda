using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
            var claim = _httpContextAccessor.HttpContext
                .User
                .Claims
                .First(_ => _.Type == ClaimType.UserId);

            return int.Parse(claim.Value, CultureInfo.InvariantCulture);
        }

        protected IEnumerable<int> GetPermissionIds()
        {
            return _httpContextAccessor.HttpContext
                .User
                .Claims
                .Where(_ => _.Type == ClaimType.PermissionId)
                .Select(_ => int.Parse(_.Value, CultureInfo.InvariantCulture));
        }

        protected bool IsSiteManager()
        {
            return _httpContextAccessor.HttpContext
                .User
                .Claims
                .Where(_ => _.Type == ClaimType.SiteManager)
                .Any();
        }
    }
}