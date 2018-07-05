using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Web;

namespace Ops.Web.WindowsAuth
{
    public class Startup
    {
        private const string HtmlDocumentHeader = "<!doctype html><html><head><style>* { font-family: sans-serif; }</style></head><body>";
        private const string HtmlDocumentFooter = "</body></html>";
        private readonly ILogger _logger;
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string redisConfiguration
                = _config[Configuration.OpsDistributedCacheRedisConfiguration]
                ?? throw new Exception($"{Configuration.OpsDistributedCacheRedisConfiguration} is not set.");
            string instanceName = CacheInstance.OcudaOps;
            services.AddDistributedRedisCache(_ =>
            {
                _.Configuration = redisConfiguration;
                _.InstanceName = instanceName;
            });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            app.Run(async (context) =>
            {
                var id = string.Empty;
                bool whoami = false;

                if (context.Request.Path.Value.StartsWith("/id/"))
                {
                    id = context.Request.Path.Value.Substring(4);
                }
                else
                {
                    whoami = context.Request.Path.Value.Contains("whoami");
                }

                if (string.IsNullOrEmpty(id) && !whoami)
                {
                    // if this site is loaded with no arguments where should it redirect?
                    var redirect = _config[Configuration.OpsAuthBlankRequestRedirect];
                    if (string.IsNullOrEmpty(redirect))
                    {
                        context.Response.ContentType = "text/html";
                        context.Response.Headers.Add("X-Robots-Tag", "noindex");
                        await context.Response.WriteAsync(HtmlDocumentHeader);
                        await context.Response.WriteAsync("&nbsp;");
                        await context.Response.WriteAsync(HtmlDocumentFooter);
                    }
                    else
                    {
                        context.Response.Redirect(redirect);
                    }
                }
                else
                {
                    var identity = context.User.Identity as WindowsIdentity;

                    var roles = ((ClaimsIdentity)context.User.Identity)
                        .Claims
                        .Where(_ => _.Type == ClaimTypes.GroupSid)
                        .Select(_ => _.Value);

                    var roleNames = roles
                        .Select(_ => new SecurityIdentifier(_)
                            .Translate(typeof(NTAccount)).ToString());

                    if (whoami)
                    {
                        _logger.LogInformation($"Displaying whoami for {identity.Name}");
                        context.Response.ContentType = "text/html";
                        context.Response.Headers.Add("X-Robots-Tag", "noindex");
                        await context.Response.WriteAsync(HtmlDocumentHeader);
                        await context.Response.WriteAsync($"<h1>You are: {identity.Name}</h1>"
                            + $"<p><em>Member of {roles.Count()} groups</em></p>"
                            + "<ul>");

                        foreach (string name in roleNames)
                        {
                            await context.Response.WriteAsync($"<li>{name}</li>");
                        }
                        await context.Response.WriteAsync("</ul>");
                        await context.Response.WriteAsync(HtmlDocumentFooter);
                    }
                    else
                    {
                        var cache = app.ApplicationServices.GetRequiredService<IDistributedCache>();

                        // by default time out cookies and distributed cache in 2 minutes
                        int authTimeoutMinutes = 2;

                        var configuredAuthTimeout = _config[Configuration.OpsAuthTimeoutMinutes];
                        if (configuredAuthTimeout != null)
                        {
                            if (!int.TryParse(configuredAuthTimeout, out authTimeoutMinutes))
                            {
                                _logger.LogWarning($"Configured {Configuration.OpsAuthTimeoutMinutes} could not be converted to a number. It should be a number of minutes (defaulting to 2).");
                            }
                        }

                        var cacheExpiration = new DistributedCacheEntryOptions()
                            .SetAbsoluteExpiration(new TimeSpan(0, authTimeoutMinutes, 0));


                        string referer
                        = await cache.GetStringAsync(string.Format(Cache.OpsReturn, id));
                        _logger.LogInformation($"Id {id} is user {identity.Name} with {roles.Count()} roles from {referer}");

                        await cache.SetStringAsync(string.Format(Cache.OpsUsername, id),
                            identity.Name,
                            cacheExpiration);

                        int groupId = 1;
                        foreach (string name in roleNames)
                        {
                            await cache.SetStringAsync(string.Format(Cache.OpsGroup, id, groupId),
                                name,
                                cacheExpiration);
                            groupId++;
                        }

                        context.Response.Redirect(referer);
                    }
                }
            });
        }
    }
}
