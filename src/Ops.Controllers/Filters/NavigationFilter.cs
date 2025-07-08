using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NavigationFilterAttribute : Attribute, IAsyncActionFilter
    {
        private const int CacheNavItemsHours = 1;

        //todo move this to configuration
        private const string NavMenuFileName = "NavMenu.json";

        public NavigationFilterAttribute(ILocationService locationService,
           ILogger<NavigationFilterAttribute> logger,
           IOcudaCache ocudaCache,
           IPathResolverService pathResolverService)
        {
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(ocudaCache);
            ArgumentNullException.ThrowIfNull(pathResolverService);

            OcudaCache = ocudaCache;
            LocationService = locationService;
            Logger = logger;
            PathResolverService = pathResolverService;
        }

        public ILocationService LocationService { get; }
        public ILogger<NavigationFilterAttribute> Logger { get; }
        public IOcudaCache OcudaCache { get; }
        public IPathResolverService PathResolverService { get; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            if (!string.IsNullOrEmpty(NavMenuFileName))
            {
                string navJson = await OcudaCache.GetStringFromCache(Utility.Keys.Cache.OpsLeftNav);
                if (string.IsNullOrEmpty(navJson))
                {
                    var navMenuJsonFile = PathResolverService
                        .GetPrivateContentFilePath(NavMenuFileName);
                    if (File.Exists(navMenuJsonFile))
                    {
                        using StreamReader file = File.OpenText(navMenuJsonFile);
                        navJson = file.ReadToEnd();
                        await OcudaCache.SaveToCacheAsync(Utility.Keys.Cache.OpsLeftNav,
                            navJson,
                            TimeSpan.FromHours(CacheNavItemsHours));
                    }
                }

                if (!string.IsNullOrEmpty(navJson))
                {
                    context.HttpContext.Items[ItemKey.NavColumn] =
                        JsonSerializer.Deserialize<List<NavigationRow>>(navJson);
                }
            }

            var locations = await OcudaCache.GetObjectFromCacheAsync<IDictionary<string, string>>(
                Utility.Keys.Cache.OpsLocationList);

            if (locations == null || locations.Count == 0)
            {
                locations = await LocationService.GetSlugNameAsync();
                if (locations?.Count > 0)
                {
                    await OcudaCache.SaveToCacheAsync(Utility.Keys.Cache.OpsLocationList,
                        locations,
                        TimeSpan.FromHours(CacheNavItemsHours));
                }
            }

            context.HttpContext.Items[ItemKey.Locations] = locations;

            await next();
        }
    }
}