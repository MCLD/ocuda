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
        private readonly AuthorizationService _authorizationService;
        private readonly SectionService _sectionService;
        private readonly UserService _userService;

        public AuthenticationFilter(ILogger<AuthenticationFilter> logger,
            IConfiguration configuration,
            IDistributedCache cache,
            AuthorizationService authorizationService,
            SectionService sectionService,
            UserService userService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _authorizationService = authorizationService
                ?? throw new ArgumentNullException(nameof(authorizationService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(SectionService));
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
                        return;
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
                        var adGroupNames = new List<string>();
                        while (!string.IsNullOrEmpty(adGroupName))
                        {
                            adGroupNames.Add(adGroupName);
                            await _cache.RemoveAsync(string.Format(Cache.OpsGroup, id, groupId));
                            groupId++;
                            adGroupName = await _cache.GetStringAsync(string.Format(Cache.OpsGroup,
                                id,
                                groupId));
                        }

                        bool isSiteManager = false;

                        var siteManagerGroups
                            = await _authorizationService.SiteManagerGroupsAsync();
                        var sectionManagerGroups
                            = await _authorizationService.SectionManagerGroupsAsync();

                        var sectionManagerOf = new List<string>();

                        foreach (string groupName in adGroupNames)
                        {
                            claims.Add(new Claim(Key.ClaimType.ADGroup, groupName));
                            if (!isSiteManager)
                            {
                                if (siteManagerGroups.Contains(groupName))
                                {
                                    isSiteManager = true;
                                }
                                else
                                {
                                    var sectionsManaged =
                                        sectionManagerGroups.Where(_ => _.GroupName == groupName);

                                    foreach (var sectionManaged in sectionsManaged)
                                    {
                                        if (!sectionManagerOf.Contains(sectionManaged.SectionName))
                                        {
                                            sectionManagerOf.Add(sectionManaged.SectionName);
                                        }
                                    }
                                }
                            }
                        }

                        if (isSiteManager)
                        {
                            claims.Add(new Claim(Key.ClaimType.SiteManager, username));
                            foreach (var section in await _sectionService.GetSectionsAsync())
                            {
                                claims.Add(new Claim(Key.ClaimType.SectionManager, 
                                    section.Name.ToLower()));
                            }
                        }
                        else
                        {
                            foreach (var sectionName in sectionManagerOf)
                            {
                                claims.Add(new Claim(Key.ClaimType.SectionManager, 
                                    sectionName.ToLower()));
                            }
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
