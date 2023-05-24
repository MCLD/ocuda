using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    public class LocationSlugRouteConstraint : IRouteConstraint
    {
        public static readonly string Name = "locationSlugConstraint";
        private readonly ILogger<LocationSlugRouteConstraint> _logger;

        public LocationSlugRouteConstraint(ILogger<LocationSlugRouteConstraint> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool Match(HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            if (values?.TryGetValue(routeKey, out var slug) == true
                && slug != null)
            {
                try
                {
                    var locationService = httpContext.RequestServices
                        .GetRequiredService<LocationService>();
                    var locationTask = locationService
                        .GetLocationIdAsync(slug.ToString(),
                            httpContext.Items[ItemKey.ForceReload] as bool? ?? false);
                    locationTask.Wait();
                    return locationTask.Result != null;
                }
                catch (ObjectDisposedException odex)
                {
                    _logger.LogCritical(odex,
                        "Error in location slug constraint: {ErrorMessage}",
                        odex.Message);
                }
                catch (AggregateException aex)
                {
                    aex.Handle(_ =>
                    {
                        _logger.LogCritical(_,
                            "Error in location slug constraint: {ErrorMessage}",
                            _.Message);
                        return true;
                    });
                }
            }

            return false;
        }
    }
}