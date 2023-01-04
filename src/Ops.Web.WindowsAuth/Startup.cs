using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services;
using Ocuda.Utility.Services.Interfaces;

namespace Ops.Web.WindowsAuth
{
    public class Startup
    {
        private const string BadConfig = "Configured {BadTimeout} could not be converted to a number. It should be a number of minutes (defaulting to 2).";
        private const string DUrlBit = "/d/";
        private const string HtmlDocumentFooter = "</body></html>";
        private const string HtmlDocumentHeader = "<!doctype html><html><head><style>* { font-family: sans-serif; }</style></head><body>";
        private const string IdUrlBit = "/id/";
        private readonly IConfiguration _config;

        public Startup(IConfiguration configuration)
        {
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        private string CacheDiscriminator { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "All exceptions should show a friendly message to the user.")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability",
            "CA1416:Validate platform compatibility",
            Justification = "This application only works on Windows.")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var logger = app.ApplicationServices.GetRequiredService<ILogger<Startup>>();
                var id = string.Empty;
                bool whoami = false;

                var path = context.Request.Path.Value;
                try
                {
                    if (path.StartsWith(IdUrlBit))
                    {
                        var dUrlBitStart = path.IndexOf(DUrlBit);
                        if (dUrlBitStart < IdUrlBit.Length)
                        {
                            id = path[IdUrlBit.Length..];
                            if (id.IndexOf("/", StringComparison.OrdinalIgnoreCase) != -1)
                            {
                                id = id[..id.IndexOf("/", StringComparison.OrdinalIgnoreCase)];
                            }
                        }
                        else
                        {
                            id = path[IdUrlBit.Length..dUrlBitStart];
                            CacheDiscriminator = path[(dUrlBitStart + DUrlBit.Length)..];
                        }
                    }
                    else
                    {
                        whoami = context
                            .Request
                            .Path
                            .Value
                            .Contains("whoami", StringComparison.OrdinalIgnoreCase);
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
                        using (Serilog.Context
                            .LogContext
                            .PushProperty("CacheDiscriminator", CacheDiscriminator))
                        {
                            var identity = context.User.Identity as WindowsIdentity;

                            var roles = ((ClaimsIdentity)context.User.Identity)
                                .Claims
                                .Where(_ => _.Type == ClaimTypes.GroupSid)
                                .Select(_ => _.Value);

                            var roleNames = roles
                                .Select(_ => new SecurityIdentifier(_)
                                    .Translate(typeof(NTAccount)).ToString());

                            string username = identity.Name;

                            string authenticationType = identity.AuthenticationType;

                            if (whoami)
                            {
                                logger.LogInformation("Displaying whoami for user {Username} with {RolesCount} roles",
                                    username,
                                    roles.Count());
                                context.Response.ContentType = "application/json";
                                context.Response.Headers.Add("X-Robots-Tag", "noindex");
                                await context.Response.WriteAsJsonAsync(new
                                {
                                    authenticatedAt = DateTime.Now,
                                    authenticationType,
                                    groupCount = roles.Count(),
                                    groups = new[] { roleNames },
                                    username
                                });
                            }
                            else
                            {
                                var cache = app.ApplicationServices.GetRequiredService<IOcudaCache>();

                                // by default time out cookies and distributed cache in 2 minutes
                                int authTimeoutMinutes = 2;

                                var configuredAuthTimeout = _config[Configuration.OpsAuthTimeoutMinutes];
                                if (configuredAuthTimeout != null
                                    && !int.TryParse(configuredAuthTimeout, out authTimeoutMinutes))
                                {
                                    logger.LogWarning(BadConfig, Configuration.OpsAuthTimeoutMinutes);
                                }

                                var cacheExpiration = new TimeSpan(0, authTimeoutMinutes, 0);

                                string referer
                                    = await cache.GetStringFromCache(CacheKey(Cache.OpsReturn, id));

                                if (roles.Any())
                                {
                                    logger.LogInformation("Id {UserId} user {Username} has {RolesCount} roles from {Referer}",
                                        id,
                                        username,
                                        roles.Count(),
                                        referer);
                                }
                                else
                                {
                                    logger.LogWarning("Id {UserId} user {Username} has {RolesCount} roles from {Referer}",
                                        id,
                                        username,
                                        roles.Count(),
                                        referer);
                                }

                                await cache.SaveToCacheAsync(CacheKey(Cache.OpsUsername, id),
                                    username,
                                    cacheExpiration);

                                int groupId = 1;
                                foreach (string roleName in roleNames)
                                {
                                    await cache.SaveToCacheAsync(CacheKey(Cache.OpsGroup, id, groupId),
                                        roleName,
                                        cacheExpiration);
                                    groupId++;
                                }

                                context.Response.Redirect(referer);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Unable to authenticate user based on path {Path}: {ErrorMessage}",
                        path,
                        ex.Message);
                    context.Response.ContentType = "text/html";
                    context.Response.Headers.Add("X-Robots-Tag", "noindex");
                    await context.Response.WriteAsync(HtmlDocumentHeader);
                    await context.Response.WriteAsync("<h1>An error occurred during authentication</h1>");
                    await context.Response.WriteAsync("<p><strong>Please contact a member of the Web team and let them know.</strong></p>");
                    await context.Response.WriteAsync("<hr><em><small>Reference: ");
                    await context.Response.WriteAsync(path);
                    await context.Response.WriteAsync("</small></em>");
                    await context.Response.WriteAsync(HtmlDocumentFooter);
                }
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string redisConfiguration
                = _config[Configuration.OpsDistributedCacheRedisConfiguration]
                ?? throw new OcudaException($"{Configuration.OpsDistributedCacheRedisConfiguration} is not set.");
            string instanceName = CacheInstance.OcudaOps;
            if (!instanceName.EndsWith("."))
            {
                instanceName += ".";
            }
            services.AddStackExchangeRedisCache(_ =>
            {
                _.Configuration = redisConfiguration;
                _.InstanceName = instanceName;
            });
            services.AddScoped<IOcudaCache, OcudaCache>();
        }

        private string CacheKey(string key, params object[] parameters)
        {
            var formatted = string.Format(CultureInfo.InvariantCulture, key, parameters);
            return string.IsNullOrEmpty(CacheDiscriminator)
                ? formatted
                : string.Format(CultureInfo.InvariantCulture,
                    "{0}.{1}",
                    CacheDiscriminator,
                    formatted);
        }
    }
}