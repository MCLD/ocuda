using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BooksByMail.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BooksByMail.Filters
{
    public class AuthenticationFilter : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<AuthenticationFilter> _logger;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly WebHelper _webHelper;

        public AuthenticationFilter(ILogger<AuthenticationFilter> logger,
            IConfiguration configuration,
            IDistributedCache cache,
            WebHelper webHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _webHelper = webHelper ?? throw new ArgumentNullException(nameof(webHelper));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var now = DateTime.Now;
            var authRedirectUrl = _config[Keys.Configuration.AuthRedirect];

            if (!string.IsNullOrEmpty(authRedirectUrl))
            {
                var httpContext = context.HttpContext;

                // check the current user's Username claim to see if they're authenticated
                var usernameClaim = httpContext.User
                    .Claims
                    .FirstOrDefault(_ => _.Type == Keys.ClaimType.Username);

                bool authenticateUser = usernameClaim == null;

                if (authenticateUser)
                {
                    // by default time out cookies and distributed cache in 2 minutes
                    int authTimeoutMinutes = 2;

                    var configuredAuthTimeout = _config[Keys.Configuration.AuthTimeoutMinutes];
                    if (configuredAuthTimeout != null)
                    {
                        if (!int.TryParse(configuredAuthTimeout, out authTimeoutMinutes))
                        {
                            _logger.LogWarning($"Configured {Keys.Configuration.AuthTimeoutMinutes} could not be converted to a number. It should be a number of minutes (defaulting to 2).");
                        }
                    }

                    // all authentication bits will expire after 2 minutes
                    var authenticationExpiration = new TimeSpan(0, authTimeoutMinutes, 0);

                    var cacheExpiration = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(authenticationExpiration);

                    // check existing authentication id cookie
                    var id = httpContext.Request.Cookies[Keys.Cookie.AuthId];
                    if (id == null)
                    {
                        // create a new authentication id cookie if none exists
                        id = Guid.NewGuid().ToString();
                        httpContext.Response.Cookies.Append(Keys.Cookie.AuthId,
                            id,
                            new Microsoft.AspNetCore.Http.CookieOptions
                            {
                                IsEssential = true,
                                MaxAge = authenticationExpiration
                            }
                        );
                    }

                    // check if there's a username stored in the cache
                    // if so, the authentication has redirected us back here

                    var username = _config[Keys.Configuration.OverrideUser];

                    if (string.IsNullOrEmpty(username))
                    {
                        username = await _cache.GetStringAsync(string.Format(Keys.Cache.Username, id));
                    }

                    if (string.IsNullOrEmpty(username))
                    {
                        // if there is no username: set a return url and redirect to authentication
                        await _cache.SetStringAsync(string.Format(Keys.Cache.Return, id),
                            _webHelper.GetCurrentUrl(httpContext),
                            cacheExpiration);

                        string cacheDiscriminator
                            = _config[Keys.Configuration.DistributedCacheInstanceDiscriminator]
                            ?? string.Empty;

                        var url = string.Format(authRedirectUrl, id, cacheDiscriminator);

                        httpContext.Response.Redirect(url);
                        return;
                    }
                    else
                    {
                        // remove the username from the cache
                        await _cache.RemoveAsync(string.Format(Keys.Cache.Username, id));

                        // check if there's a domain name specified and strip it from the username
                        var domainName = _config[Keys.Configuration.DomainName];
                        if (!string.IsNullOrEmpty(domainName) && username.StartsWith(domainName))
                        {
                            username = username[(domainName.Length + 1)..];
                        }

                        // start creating the user's claims with their username
                        var claims = new HashSet<Claim>
                        {
                            new Claim(Keys.ClaimType.Username, username),
                        };

                        // TODO: probably change the role claim type to our roles and not AD groups
                        var identity = new ClaimsIdentity(claims,
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            Keys.ClaimType.Username,
                            null);

                        await httpContext.SignInAsync(new ClaimsPrincipal(identity));

                        // remove the return URL from the cache
                        await _cache.RemoveAsync(string.Format(Keys.Cache.Return, id));

                        httpContext.Response.Cookies.Delete(Keys.Cookie.AuthId);

                        // TODO set a reasonable initial nickname
                        httpContext.Items[Keys.Item.Username] = username;
                    }
                }
            }

            await next();
        }
    }
}
