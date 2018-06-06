using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace Ocuda.Ops.Web.Middleware
{
    public class OpsAuthentication
    {
        private readonly RequestDelegate _next;
        private readonly IDistributedCache _cache;
        public OpsAuthentication(RequestDelegate next, IDistributedCache cache)
        {
            _next = next ?? throw new ArgumentNullException();
            _cache = cache ?? throw new ArgumentNullException();
        }
        public async Task Invoke(HttpContext httpContext)
        {
            if(_cache.Get($"auth.{httpContext.Session.Id}") == null)
            {
                // TODO: redirect to authentication
                //httpContext.Response.Redirect($"http://authentication/{httpContext.Session.Id}");
                //return;
            }

            await _next.Invoke(httpContext);
        }
    }

    public static class OpsAuthenticationExtensions
    {
        public static IApplicationBuilder UseOpsAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<OpsAuthentication>();
        }
    }
}
