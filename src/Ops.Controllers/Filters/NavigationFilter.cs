using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Filters
{
    public class NavigationFilter : Attribute, IAsyncActionFilter
    {
        private readonly ILogger<NavigationFilter> _logger;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;

        public NavigationFilter(ILogger<NavigationFilter> logger,
           IConfiguration config,
           IDistributedCache cache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, 
            ActionExecutionDelegate next)
        {
            context.HttpContext.Items[ItemKey.NavColumn] = 
                _config.GetSection(Configuration.OpsNavColumn).Get<List<NavigationRow>>();

            await next();
        }
    }
}
