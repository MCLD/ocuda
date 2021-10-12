using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Ops.Models;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Filters
{
    public class NavigationFilterAttribute : Attribute, IAsyncActionFilter
    {
        //todo move this to configuration
        private const string NavMenuFileName = "NavMenu.json";

        private readonly IOcudaCache _cache;
        private readonly ILogger<NavigationFilterAttribute> _logger;
        private readonly IPathResolverService _pathResolverService;

        public NavigationFilterAttribute(ILogger<NavigationFilterAttribute> logger,
           IOcudaCache cache,
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
            if (!string.IsNullOrEmpty(NavMenuFileName))
            {
                string navJson = await _cache.GetStringFromCache(Cache.OpsLeftNav);
                if (string.IsNullOrEmpty(navJson))
                {
                    var navMenuJsonFile = _pathResolverService.GetPrivateContentFilePath(NavMenuFileName);
                    if (File.Exists(navMenuJsonFile))
                    {
                        using StreamReader file = File.OpenText(navMenuJsonFile);
                        navJson = file.ReadToEnd();
                        await _cache.SaveToCacheAsync(Cache.OpsLeftNav,
                            navJson,
                            TimeSpan.FromHours(1));
                    }
                }

                if (!string.IsNullOrEmpty(navJson))
                {
                    context.HttpContext.Items[ItemKey.NavColumn] =
                        JsonConvert.DeserializeObject<List<NavigationRow>>(navJson);
                }
            }
            await next();
        }
    }
}