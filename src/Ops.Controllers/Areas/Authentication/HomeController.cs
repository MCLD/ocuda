using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Authentication.ViewModels;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using Serilog.Context;

namespace Ocuda.Ops.Controllers.Areas.Authentication
{
    [Area(nameof(Authentication))]
    [Route("[area]")]
    public class HomeController : BaseUnauthenticatedController<Controllers.HomeController>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIdentityProviderService _identityProviderService;
        private readonly ILdapService _ldapService;
        private readonly ISamlService _samlService;
        private readonly IUserManagementService _userManagementService;
        private readonly IUserService _userService;

        public HomeController(Controller<Controllers.HomeController> context,
            IIdentityProviderService identityProviderService,
            IDateTimeProvider dateTimeProvider,
            ILdapService ldapService,
            ISamlService samlService,
            IAuthorizationService authorizationService,
            IUserManagementService userManamagementService,
            IUserService userService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(identityProviderService);
            ArgumentNullException.ThrowIfNull(ldapService);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(authorizationService);
            ArgumentNullException.ThrowIfNull(userService);
            ArgumentNullException.ThrowIfNull(samlService);
            ArgumentNullException.ThrowIfNull(userManamagementService);

            _userManagementService = userManamagementService;
            _dateTimeProvider = dateTimeProvider;
            _identityProviderService = identityProviderService;
            _ldapService = ldapService;
            _userService = userService;
            _samlService = samlService;
            _authorizationService = authorizationService;
        }

        public static string Area
        { get { return nameof(Authentication); } }

        public static string Name
        { get { return "Home"; } }

        [HttpGet("[action]")]
        public IActionResult Direct()
        {
            HttpContext.Session.SetBoolean(Session.SkipAutoIdentityProvider, true);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string returnUrl)
        {
            var adminEmail = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Email.AdminAddress);

            string mailLink = null;
            if (!string.IsNullOrEmpty(adminEmail))
            {
                mailLink = $"mailto:{adminEmail}?subject="
                    + Uri.EscapeDataString("Difficulty accessing the intranet")
                    + "&body="
                    + Uri.EscapeDataString("I am experiencing difficulty authenticating to the intranet: ");
            }

            var viewModel = new LoginViewModel
            {
                AdminEmail = mailLink,
                ReturnUrl = returnUrl
            };

            if (User?.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("Authenticated user {Username} tried to access login screen with return link set to {ReturnUrl}",
                    User?.Identity?.Name,
                    returnUrl);

                viewModel.AuthenticatedUser = User?.Identity?.Name;
                return View("Info", viewModel);
            }

            // if there's a default provider configured we'll automatically try that
            var activeProviders = await _identityProviderService.GetAllActiveAsync();

            // session value is populated with a 1 in the first byte then skip default provider
            var skipProvider = HttpContext.Session.GetBoolean(Session.SkipAutoIdentityProvider);

            if (!skipProvider)
            {
                var defaultProvider = activeProviders?.FirstOrDefault(_ => _.IsDefault);
                if (defaultProvider != null)
                {
                    // set the skip default in case something goes wrong externally
                    HttpContext.Session.SetBoolean(Session.SkipAutoIdentityProvider, true);
                    return Redirect(ISamlService.GenerateRedirectLink(defaultProvider, returnUrl));
                }
            }

            foreach (var activeProvider in activeProviders)
            {
                viewModel.ProviderLinks.Add(activeProvider.Name,
                    ISamlService.GenerateRedirectLink(activeProvider, returnUrl));
            }

            return View("Login", viewModel);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> JustLogout()
        {
            _logger.LogInformation("Logging out user {Username}",
                HttpContext.User?.Identity?.Name ?? "unauthenticated user");

            await HttpContext.SignOutAsync();
            return Ok();
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login(LoginViewModel viewModel, string returnUrl)
        {
            if (viewModel == null
                 || string.IsNullOrEmpty(viewModel.Username)
                 || string.IsNullOrEmpty(viewModel.Password))
            {
                ShowAlertWarning("Unable to login.");
                return RedirectToAction(nameof(Unauthorized));
            }

            var result = _ldapService.VerifyCredentials(viewModel.Username, viewModel.Password);

            if (result != null)
            {
                await LoginUser(result.Username,
                    IdentityProviderType.Form,
                    Request.GetDisplayUrl());

                if (!string.IsNullOrWhiteSpace(viewModel?.ReturnUrl)
                    || !string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(viewModel.ReturnUrl ?? returnUrl);
                }
                else
                {
                    _logger.LogInformation("User {Username} logged in via form", result.Username);
                    return RedirectToHome();
                }
            }

            // if a form submit failed, do not redirect to a SAML provider
            HttpContext.Session.SetBoolean(Session.SkipAutoIdentityProvider, true);

            ShowAlertWarning("Login failed.");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("[action]")]
        public async Task<RedirectToActionResult> Logout()
        {
            _logger.LogInformation("Logging out user {Username}",
                HttpContext.User?.Identity?.Name ?? "unauthenticated user");

            await HttpContext.SignOutAsync();
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("saml/{provider}")]
        public IActionResult SamlAuthenticateGet(string provider)
        {
            if (string.IsNullOrEmpty(provider))
            {
                _logger.LogError("Authenticate was called via HTTP GET but no provider was specified.");
            }
            else
            {
                _logger.LogError("Authenticate was called via HTTP GET for provider {Provider}",
                    provider);
            }
            return StatusCode(StatusCodes.Status400BadRequest);
        }

        [HttpPost("saml/{provider}")]
        public async Task<IActionResult> SamlAuthenticatePost(string provider, string relayState)
        {
            if (string.IsNullOrEmpty(provider))
            {
                _logger.LogError("Authenticate was called but no provider was specified.");
                return LoginFailure("SAML authentication failed.");
            }

            using (LogContext.PushProperty(Utility.Logging.Enrichment.AuthenticationProvider,
                provider))
            {
                var activeProvider = await _identityProviderService.GetActiveAsync(provider.Trim());

                if (activeProvider == null)
                {
                    _logger.LogError("SAML provider {Provider} was requested and is not available.",
                        provider);
                    return LoginFailure("SAML authentication failed.");
                }

                if (Request?.ContentLength == null || Request.ContentLength == 0)
                {
                    _logger.LogError("SAML provider returned a null response.");
                    return LoginFailure("SAML authentication failed.");
                }

                StringValues? postedData = null;

                try
                {
                    postedData = Request.Form[activeProvider.FormName];
                }
                catch (InvalidOperationException ioex)
                {
                    _logger.LogError(ioex,
                        "SAML provider returned a bad form: {ErrorMessage}",
                        ioex.Message);
                    return LoginFailure("SAML authentication failed.");
                }

                if (!postedData.HasValue)
                {
                    _logger.LogError("SAML provider authentication request contained no data.");
                    return LoginFailure("SAML authentication failed.");
                }

                IdentityResponse response = null;
                try
                {
                    response = _samlService.ValidateLogin(activeProvider, postedData.Value);
                }
                catch (Utility.Exceptions.OcudaException oex)
                {
                    _logger.LogError(oex,
                        "Login validation from SAML request failed: {ErrorMessage}",
                        oex.Message);
                    return LoginFailure("SAML authentication failed.");
                }

                if (response?.IsValid == true)
                {
                    await LoginUser(response.UserId,
                        activeProvider.ProviderType,
                        activeProvider.Name);

                    if (!string.IsNullOrEmpty(relayState))
                    {
                        _logger.LogInformation("User {Username} logged in via SAML, redirecting to: {RelayState}",
                            response.UserId,
                            relayState);
                        return Redirect(relayState);
                    }

                    _logger.LogInformation("User {Username} logged in via SAML.",
                        response.UserId);
                    return RedirectToHome();
                }
                else
                {
                    _logger.LogError("SAML provider returned an unauthorized user.");
                    return LoginFailure("SAML authentication failed.");
                }
            }
        }

        private RedirectToActionResult LoginFailure(string failureMessage)
        {
            _logger.LogInformation("SAML authentication failure, telling user {Message} and skipping SAML on next attempt",
                failureMessage);
            ShowAlertWarning(failureMessage);
            HttpContext.Session.SetBoolean(Session.SkipAutoIdentityProvider, true);
            return RedirectToAction(nameof(Index));
        }

        private async Task LoginUser(string username,
            IdentityProviderType providerType,
            string providerName)
        {
            var now = _dateTimeProvider.Now;
            var user = await _userService.LookupUserAsync(username);

            var newUser = user == null;
            if (newUser)
            {
                user = new User
                {
                    Username = username,
                    LastSeen = now
                };
            }

            // perform ldap update of user object
            var lookupUser = _ldapService.LookupByUsername(user);
            if (lookupUser != null)
            {
                user = lookupUser;
            }
            else
            {
                _logger.LogWarning("Unable to find username {Username} in LDAP", user.Username);
            }

            // if the user is new, add them to the database
            if (newUser)
            {
                var rosterUser = await _userService.LookupUserByEmailAsync(user.Email);
                if (rosterUser != null)
                {
                    _logger.LogInformation("New user {Username} in database email: {Email}",
                        user.Username,
                        user.Email);
                    user = await _userManagementService
                        .UpdateRosterUserAsync(rosterUser.Id, user);
                }
                else
                {
                    _logger.LogInformation("New user {Username} inserting into database",
                        user.Username);
                    user = await _userManagementService.AddUser(user);
                }
            }
            else
            {
                await _userManagementService.LoggedInUpdateAsync(user);
            }

            var userId = user.Id.ToString(CultureInfo.InvariantCulture);

            // start creating the user's claims with their username
            var claims = new HashSet<Claim>
            {
                new(ClaimType.AuthenticatedAt, now.ToString("O",CultureInfo.InvariantCulture)),
                new(ClaimType.IdentityProvider, providerName),
                new(ClaimType.IdentityProviderType,
                    Enum.GetName(typeof(IdentityProviderType),
                    providerType)),
                new(ClaimType.UserId, userId),
                new(ClaimTypes.Name, username)
            };

            bool isSiteManager = false;

            // pull lists of AD groups that should be site managers
            var claimGroups = await _authorizationService.GetClaimGroupsAsync();
            var permissionGroups = await _authorizationService.GetPermissionGroupsAsync();

            var claimantOf = new Dictionary<string, string>();
            var inPermissionGroup = new List<int>();

            // loop through group names and look up if each group provides claims
            // claims can be provided via ClaimGroups
            foreach (string groupName in user.SecurityGroups)
            {
                claims.Add(new Claim(ClaimType.ADGroup, groupName));

                // once the user is a site manager, we can stop looking up more rights
                if (!isSiteManager)
                {
                    foreach (var claim in claimGroups
                        .Where(_ => _.GroupName == groupName))
                    {
                        claimantOf.TryAdd(claim.ClaimType, groupName);
                    }

                    var permissionList = permissionGroups
                        .Where(_ => _.GroupName == groupName);
                    foreach (var permission in permissionList)
                    {
                        inPermissionGroup.Add(permission.Id);
                    }

                    if (claimantOf.ContainsKey(ClaimType.SiteManager))
                    {
                        isSiteManager = true;
                    }
                }
            }

            if (isSiteManager)
            {
                // also add each individual permission claim
                foreach (var claimType in claimGroups)
                {
                    claims.Add(new Claim(claimType.ClaimType,
                        ClaimType.SiteManager));
                }

                foreach (var permissionId in permissionGroups.Select(_ => _.Id))
                {
                    claims.Add(new Claim(ClaimType.PermissionId,
                        permissionId.ToString(CultureInfo.InvariantCulture)));
                }

                claims.Add(new Claim(ClaimType.HasContentAdminRights,
                    ClaimType.HasContentAdminRights));
                claims.Add(new Claim(ClaimType.HasSiteAdminRights,
                    ClaimType.HasSiteAdminRights));
            }
            else
            {
                // add permission claims
                foreach (var claim in claimantOf)
                {
                    claims.Add(new Claim(claim.Key, claim.Value));
                }

                foreach (var permissionId in inPermissionGroup)
                {
                    claims.Add(new Claim(ClaimType.PermissionId,
                        permissionId.ToString(CultureInfo.InvariantCulture)));
                }

                var hasAdminClaims = await _authorizationService
                    .GetAdminClaimsAsync(inPermissionGroup);

                foreach (var hasAdminClaim in hasAdminClaims)
                {
                    claims.Add(new Claim(hasAdminClaim, hasAdminClaim));
                }
            }

            // TODO: probably change the role claim type to our roles and not AD groups
            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme,
                ClaimTypes.Name,
                ClaimType.ADGroup);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true
                });

            // TODO set a reasonable initial nickname
            HttpContext.Items[ItemKey.Nickname] = user.Nickname ?? username;
        }
    }
}