using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Controllers.Areas.ContentManagement;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Serilog.Context;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class ApiController : Controller
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDigitalDisplayService _digitalDisplayService;
        private readonly HttpClient _httpClient;
        private readonly ILdapService _ldapService;
        private readonly ILogger _logger;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IUserService _userService;

        public ApiController(IApiKeyService apiKeyService,
            IAuthorizationService authorizationService,
            IDigitalDisplayService digitalDisplayService,
            HttpClient httpClient,
            ILdapService ldapService,
            ILogger<ApiController> logger,
            IPermissionGroupService permissionGroupService,
            ISiteSettingService siteSettingService,
            IUserService userService)
        {
            ArgumentNullException.ThrowIfNull(apiKeyService);
            ArgumentNullException.ThrowIfNull(authorizationService);
            ArgumentNullException.ThrowIfNull(digitalDisplayService);
            ArgumentNullException.ThrowIfNull(httpClient);
            ArgumentNullException.ThrowIfNull(ldapService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(siteSettingService);
            ArgumentNullException.ThrowIfNull(userService);

            _apiKeyService = apiKeyService;
            _authorizationService = authorizationService;
            _digitalDisplayService = digitalDisplayService;
            _httpClient = httpClient;
            _ldapService = ldapService;
            _logger = logger;
            _permissionGroupService = permissionGroupService;
            _siteSettingService = siteSettingService;
            _userService = userService;
        }

        public static string Name
        { get { return "Api"; } }

        private static JsonResponse ErrorJobResult(string message)
        {
            return new JsonResponse
            {
                Message = message,
                ServerResponse = true,
                Success = false
            };
        }

        public async Task<AddressLookupResult> AddressLookup(string address, string zip)
        {
            var addressLookupPath = await _siteSettingService.GetSettingStringAsync(Models
                    .Keys
                    .SiteSetting
                    .RenewCard
                    .AddressLookupUrl);

            var queryParams = new Dictionary<string, string>
            {
                { nameof(address), HttpUtility.UrlEncode(address) },
                { nameof(zip), HttpUtility.UrlEncode(zip) }
            };
            var parameterString = string.Join('&', queryParams.Select(_ => $"{_.Key}={_.Value}"));

            var queryUri = new UriBuilder(addressLookupPath) { Query = parameterString }.Uri;

            using var response = await _httpClient.GetAsync(queryUri);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                
                try
                {
                    return await JsonSerializer.DeserializeAsync<AddressLookupResult>(
                        responseStream,
                        new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                }
                catch (JsonException jex)
                {
                    _logger.LogError(jex, "Error decoding JSON: {ErrorMessage}", jex.Message);
                }
            }
            else
            {
                _logger.LogError("Address lookup returned status code {StatusCode} for parameters {Parameters}",
                    response.StatusCode,
                    parameterString);
            }

            return null;
        }

        #region Slide Upload

        [HttpPost("[action]")]
        public async Task<IActionResult> UploadJob(SlideUploadJob job)
        {
            ApiKey apiKey = null;
            try
            {
                if (job == null)
                {
                    throw new OcudaException("Empty API key request.");
                }
                apiKey = await _apiKeyService.FindAsync(job.ApiKey)
                    ?? throw new OcudaException("API key not found.");
            }
            catch (OcudaException oex)
            {
                return Json(ErrorJobResult(oex.Message));
            }

            // api key success below here
            var user = await _userService.GetByIdAsync(apiKey.RepresentsUserId);
            if (user == null)
            {
                return Json(ErrorJobResult("User not found."));
            }

            var lookupUser = _ldapService.LookupByUsername(user);
            if (lookupUser == null)
            {
                return Json(ErrorJobResult("User access not found."));
            }

            using (LogContext.PushProperty("APIActingOnBehalfOf", lookupUser.Username))
            {
                var claimGroups = await _authorizationService.GetClaimGroupsAsync();

                bool isSiteManager = false;

                // site manager lookup
                var siteManagerGroup = claimGroups.Where(_ => _.ClaimType == ClaimType.SiteManager)
                    .Select(_ => _.GroupName)
                    .SingleOrDefault();

                if (!string.IsNullOrEmpty(siteManagerGroup))
                {
                    isSiteManager = user.SecurityGroups.Contains(siteManagerGroup);
                }

                bool hasDigitalDisplayPermission = false;

                // if not site manager, then digital display permission lookup
                if (!isSiteManager)
                {
                    var appPermissionGroups = await _permissionGroupService
                        .GetApplicationPermissionGroupsAsync(ApplicationPermission
                            .DigitalDisplayContentManagement);

                    if (appPermissionGroups.Count != 0)
                    {
                        hasDigitalDisplayPermission = user.SecurityGroups
                            .Any(_ => appPermissionGroups
                                .Any(__ => _.Contains(__.GroupName,
                                    StringComparison.OrdinalIgnoreCase)));
                    }
                }

                if (!isSiteManager && !hasDigitalDisplayPermission)
                {
                    return Json(ErrorJobResult("You do not have permission to upload an asset."));
                }

                // validate job
                if (job == null)
                {
                    return Json(ErrorJobResult("No job submitted."));
                }

                if (job.StartDate == default)
                {
                    return Json(ErrorJobResult("No start date specified."));
                }

                if (job.EndDate == default)
                {
                    return Json(ErrorJobResult("No end date specified."));
                }

                if (string.IsNullOrEmpty(job.Set))
                {
                    return Json(ErrorJobResult("No display set specified."));
                }

                var set = await _digitalDisplayService.GetSetAsync(job.Set.Trim());

                if (set == null)
                {
                    return Json(ErrorJobResult($"Could not find set: {job.Set}"));
                }

                DigitalDisplayAsset asset;

                try
                {
                    asset = await _digitalDisplayService.UploadAssetAsync(job.File,
                        apiKey.RepresentsUserId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unable to add digital display asset for {Username}: {ErrorMessage}",
                        user.Username,
                        ex.Message);
                    return Json(ErrorJobResult($"Error uploading image: {ex.Message}"));
                }

                try
                {
                    var startDate = job.TimeZoneOffsetMinutes == default
                        ? job.StartDate
                        : job.StartDate.AddMinutes(job.TimeZoneOffsetMinutes * -1);

                    var endDate = job.TimeZoneOffsetMinutes == default
                        ? job.EndDate
                        : job.EndDate.AddMinutes(job.TimeZoneOffsetMinutes * -1);

                    await _digitalDisplayService
                        .AddUpdateDigitalDisplayAssetSetAsync(new DigitalDisplayAssetSet
                        {
                            DigitalDisplayAssetId = asset.Id,
                            DigitalDisplaySetId = set.Id,
                            EndDate = endDate,
                            StartDate = startDate,
                            IsEnabled = true
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unable to add asset to digital display set for {Username}: {ErrorMessage}",
                        user.Username,
                        ex.Message);
                    return Json(ErrorJobResult($"Error adding digital display asset: {ex.Message}"));
                }

                _logger.LogInformation("Added display asset for {Username}, asset {Asset} to set {Set}",
                    user.Username,
                    asset.Name,
                    set.Name);

                return Json(new JsonResponse
                {
                    Url = Url.Action(nameof(DigitalDisplaysController.AssetAssociations),
                    DigitalDisplaysController.Name,
                    new
                    {
                        area = DigitalDisplaysController.Area,
                        digitalDisplayAssetId = asset.Id
                    }),
                    Message = $"Added asset id {asset.Id} to set id {set.Id}",
                    ServerResponse = true,
                    Success = true
                });
            }
        }

        #endregion Slide Upload
    }
}