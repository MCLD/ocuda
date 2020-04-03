using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Ops.Models;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Filters
{
    public class NavigationFilterAttribute : Attribute, IAsyncActionFilter
    {
        private const string NavMenuFileName = "NavMenu.json";

        private readonly ILogger<NavigationFilterAttribute> _logger;
        private readonly IDistributedCache _cache;
        private readonly IPathResolverService _pathResolverService;

        public NavigationFilterAttribute(ILogger<NavigationFilterAttribute> logger,
           IDistributedCache cache,
           IPathResolverService pathResolverService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _pathResolverService = pathResolverService
                ?? throw new ArgumentNullException(nameof(pathResolverService));
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var navMenuJsonFile = _pathResolverService.GetPrivateContentFilePath(NavMenuFileName);
            using (StreamReader file = File.OpenText(navMenuJsonFile))
            {
                var serializer = new JsonSerializer();
                context.HttpContext.Items[ItemKey.NavColumn] = (List<NavigationRow>)serializer
                    .Deserialize(file, typeof(List<NavigationRow>));
            }

            await next();
        }
    }
}
