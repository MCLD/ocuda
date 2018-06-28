using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Web;

namespace Ocuda.Ops.Controllers.Filter
{
    public class AuthenticationFilter : Attribute, IAsyncResourceFilter
    {
        private readonly ILogger<AuthenticationFilter> _logger;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;
        private readonly UserService _userService;

        public AuthenticationFilter(ILogger<AuthenticationFilter> logger,
            IConfiguration configuration,
            IDistributedCache cache,
            UserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task OnResourceExecutionAsync(ResourceExecutingContext context,
            ResourceExecutionDelegate next)
        {
            var authRedirectUrl = _config[Configuration.OpsAuthRedirect];

            if (!string.IsNullOrEmpty(authRedirectUrl))
            {
                var httpContext = context.HttpContext;

                var usernameClaim = httpContext.User
                    .Claims
                    .Where(_ => _.Type == Key.ClaimType.Username)
                    .FirstOrDefault();

                bool authenticateUser = usernameClaim == null;

                if (!authenticateUser)
                {
                    // user is logged in, ensure they exist in the database
                    string username = usernameClaim.Value;
                    var user = await _userService.LookupUser(username);
                    if (user == null || user.ReauthenticateUser)
                    {
                        // user does not exist in the database or needs to be reauthenticated
                        authenticateUser = true;
                        await httpContext.SignOutAsync();
                    }
                }

                if (authenticateUser)
                {
                    var id = httpContext.Request.Cookies[Cookie.OpsAuthId];
                    if (id == null)
                    {
                        id = Guid.NewGuid().ToString();
                        httpContext.Response.Cookies.Append(Cookie.OpsAuthId, id);
                    }
                    var username
                        = await _cache.GetStringAsync(string.Format(Cache.OpsUsername, id));

                    if (string.IsNullOrEmpty(username))
                    {
                        await _cache.SetStringAsync(string.Format(Cache.OpsReturn, id),
                            new Helper().GetCurrentUrl(httpContext));

                        httpContext.Response.Redirect(string.Format(authRedirectUrl, id));
                    }
                    else
                    {
                        await _cache.RemoveAsync(string.Format(Cache.OpsUsername, id));

                        var domainName = _config[Configuration.OpsDomainName];

                        if (!string.IsNullOrEmpty(domainName) && username.StartsWith(domainName))
                        {
                            username = username.Substring(domainName.Length + 1);
                        }

                        var user = await _userService.LookupUser(username);
                        if (user == null)
                        {
                            await _userService.AddUser(new Models.User
                            {
                                Username = username
                            });
                        }

                        var claims = new HashSet<Claim>();
                        claims.Add(new Claim(Key.ClaimType.Username, username));
                        int groupId = 1;
                        var adGroupName
                            = await _cache.GetStringAsync(string.Format(Cache.OpsGroup,
                                id,
                                groupId));

                        while (!string.IsNullOrEmpty(adGroupName))
                        {
                            claims.Add(new Claim(Key.ClaimType.ADGroup, adGroupName));
                            await _cache.RemoveAsync(string.Format(Cache.OpsGroup, id, groupId));
                            groupId++;
                            adGroupName = await _cache.GetStringAsync(string.Format(Cache.OpsGroup,
                                id,
                                groupId));
                        }

                        // TODO: probably change the role claim type to our roles and not AD groups
                        var identity = new ClaimsIdentity(claims,
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            Key.ClaimType.Username,
                            Key.ClaimType.ADGroup);

                        await httpContext.SignInAsync(new ClaimsPrincipal(identity));

                        httpContext.Response.Cookies.Delete(Cookie.OpsAuthId);

                        // TODO set a reasonable initial nickname
                        httpContext.Items[ItemKey.Nickname] = username;
                    }
                }
            }

            await next();
        }
    }
}
