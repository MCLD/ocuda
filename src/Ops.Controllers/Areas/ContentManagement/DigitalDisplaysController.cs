using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement
{
    [Area("ContentManagement")]
    [Route("[area]/[controller]")]
    public class DigitalDisplaysController : BaseController<DigitalDisplaysController>
    {
        private readonly IDigitalDisplayService _digitalDisplayService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;

        public DigitalDisplaysController(
            ServiceFacades.Controller<DigitalDisplaysController> context,
            IDigitalDisplayService digitalDisplayService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _digitalDisplayService = digitalDisplayService
                ?? throw new ArgumentNullException(nameof(digitalDisplayService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Name { get { return "DigitalDisplays"; } }

        [HttpGet]
        [Route("[action]")]
        [SaveModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public IActionResult AddSet()
        {
            return View("AddUpdateSet");
        }

        [HttpPost]
        [Route("[action]")]
        [RestoreModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddUpdateSet(DigitalDisplaySet digitalDisplaySet)
        {
            if (ModelState.IsValid && digitalDisplaySet != null)
            {
                if (digitalDisplaySet.Id == default)
                {
                    await _digitalDisplayService.AddSetAsync(digitalDisplaySet);
                }
                else
                {
                    await _digitalDisplayService.UpdateSetAsync(digitalDisplaySet);
                }
                return RedirectToAction(nameof(Sets));
            }

            return RedirectToAction(nameof(AddSet));
        }

        [HttpGet]
        [Route("[action]/{digitalDisplayAssetId}")]
        public async Task<IActionResult> AssetAssociations(int digitalDisplayAssetId)
        {
            if (!await HasContentManagementRightsAsync())
            {
                ShowAlertDanger("You do not have permission to associate assets.");
                return RedirectToAction(nameof(Assets));
            }

            var asset = await _digitalDisplayService.GetAssetAsync(digitalDisplayAssetId);

            return View("AssetAssociations", new AssetAssociationViewModel
            {
                DigitalDisplayAsset = asset,
                DigitalDisplayAssetSets = await _digitalDisplayService.GetAssetSetsAsync(asset.Id),
                DigitalDisplaySets = await _digitalDisplayService.GetSetsAsync(),
                ImageUrl = _digitalDisplayService.GetAssetWebPath(asset)
            });
        }

        [HttpGet]
        [Route("[action]")]
        [Route("[action]/{page}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Assets(int page)
        {
            page = page == default ? 1 : page;

            var filter = new BaseFilter(page);

            var assets = await _digitalDisplayService.GetPaginatedAssetsAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = assets.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = paginateModel.LastPage ?? 1 });
            }

            var assetCounts = await _digitalDisplayService
                .GetAssetSetsCountAsync(assets.Data.Select(_ => _.Id));

            return View(new AssetListViewModel
            {
                DigitalDisplayAssets = assets.Data,
                DigitalDisplaySetAssets = assetCounts,
                HasDigitalDisplayPermissions = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.DigitalDisplayContentManagement),
                ImageUrls = _digitalDisplayService.GetAssetUrlPaths(assets.Data),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                PaginateModel = paginateModel
            });
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AssignSet([FromBody] UpdateSetModel viewmodel)
        {
            if (viewmodel == null)
            {
                return Json(new UpdateSetModel
                {
                    ErrorMessage = "No display and set provided."
                });
            }

            IEnumerable<int> sets;
            try
            {
                sets = await _digitalDisplayService
                    .AssignSetAsync(viewmodel.DisplayId, viewmodel.SetId);
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Unable to update display {DisplayId} for set {SetId}: {ErrorMessage}",
                    viewmodel.DisplayId,
                    viewmodel.SetId,
                    oex.Message);
                return Json(new UpdateSetModel
                {
                    ErrorMessage = oex.Message
                });
            }
            return Json(new UpdateSetModel
            {
                Success = true,
                SetIds = sets
            });
        }

        [HttpGet]
        [Route("[action]/{displayId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AssignSets(int displayId)
        {
            var displaysSets = await _digitalDisplayService
                .GetDisplaysSetsAsync(new[] { displayId });

            return View(new AssignSetsViewModel
            {
                DigitalDisplay = await _digitalDisplayService.GetDisplayAsync(displayId),
                DisplaySets = displaysSets.Select(_ => _.DigitalDisplaySetId),
                Sets = await _digitalDisplayService.GetSetsAsync()
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteAsset(int digitalDisplayAssetId)
        {
            if (!await HasContentManagementRightsAsync())
            {
                ShowAlertDanger("You do not have permission to remove this asset.");
                return RedirectToAction(nameof(Assets));
            }

            try
            {
                await _digitalDisplayService.DeleteAssetAsync(digitalDisplayAssetId);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger(oex.Message);
            }
            return RedirectToAction(nameof(Assets));
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeleteDisplay(int digitalDisplayId)
        {
            await _digitalDisplayService.DeleteDisplayAsync(digitalDisplayId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeleteSet(int digitalDisplaySetId)
        {
            await _digitalDisplayService.DeleteSetAsync(digitalDisplaySetId);
            return RedirectToAction(nameof(Sets));
        }

        [Route("")]
        public async Task<IActionResult> Index()
        {
            var locations = await _locationService.GetAllLocationsAsync();
            var displays = await _digitalDisplayService.GetDisplaysAsync();
            var sets = await _digitalDisplayService.GetSetsAsync();
            var displaysSets = await _digitalDisplayService
                .GetDisplaysSetsAsync(displays.Select(_ => _.Id));

            var displaySetNames = new Dictionary<int, string>();
            foreach (var display in displays)
            {
                var setIds = displaysSets.Where(_ => _.DigitalDisplayId == display.Id)
                    .Select(_ => _.DigitalDisplaySetId);
                displaySetNames.Add(display.Id,
                    string.Join(", ",
                        sets.Where(_ => setIds.Contains(_.Id)).Select(_ => _.Name)));
            }

            return View(new DisplayListViewModel
            {
                DigitalDisplays = displays,
                DisplaySetNames = displaySetNames,
                HasDigitalDisplayPermissions = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.DigitalDisplayContentManagement),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                Locations = locations.ToDictionary(k => k.Id, v => v.Name)
            });
        }

        [HttpGet]
        [Route("[action]")]
        [RestoreModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Provision()
        {
            var digitalDisplaySets = await _digitalDisplayService.GetSetsAsync();

            if (digitalDisplaySets.Count == 0)
            {
                AlertWarning = "Before provisioning a display you must have at least one digital display content set configured.";
                return RedirectToAction(nameof(Sets));
            }

            return View(new ProvisionDisplayViewModel
            {
                DigitalDisplaySets = SetDropDown(digitalDisplaySets),
                Locations = LocationDropDown(await _locationService.GetAllLocationsAsync())
            });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Provision(ProvisionDisplayViewModel viewmodel)
        {
            if (ModelState.IsValid && viewmodel != null)
            {
                await _digitalDisplayService.AddDisplayAsync(viewmodel.DigitalDisplay);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Provision));
        }

        [HttpGet]
        [Route("[action]")]
        [SaveModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Sets()
        {
            return View(new SetListViewModel
            {
                DigitalDisplaySets = await _digitalDisplayService.GetSetsAsync(),
                DigitalDisplaySetAssets = await _digitalDisplayService.GetSetsAssetCountsAsync(),
                DigitalDisplaySetDisplays
                    = await _digitalDisplayService.GetSetsDisplaysCountsAsync(),
                HasDigitalDisplayPermissions = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.DigitalDisplayContentManagement),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult>
            UpdateAssetAssocations([FromBody] UpdatedAssetAssociationsModel update)
        {
            if (!await HasContentManagementRightsAsync())
            {
                return Json(new UpdatedAssetAssociationsModel
                {
                    Success = false,
                    Message = "You do not have permission to alter asset assocations."
                });
            }

            if (update == null)
            {
                update = new UpdatedAssetAssociationsModel
                {
                    Success = false,
                    Message = "No update was submitted."
                };
            }
            else
            {
                try
                {
                    if (update.Enabled)
                    {
                        await _digitalDisplayService
                            .AddUpdateDigitalDisplayAssetSetAsync(new DigitalDisplayAssetSet
                            {
                                DigitalDisplayAssetId = update.AssetId,
                                DigitalDisplaySetId = update.SetId,
                                EndDate = update
                                    .EndDateTimeUTC
                                    .AddMinutes(update.TimeZoneOffsetMinutes * -1),
                                StartDate = update
                                    .StartDateTimeUTC
                                    .AddMinutes(update.TimeZoneOffsetMinutes * -1),
                                IsEnabled = update.Enabled
                            });
                        update.Success = true;
                        update.Message = "Start date is "
                            + update.StartDateTimeUTC.AddMinutes(update.TimeZoneOffsetMinutes * -1)
                            + '.';
                    }
                    else
                    {
                        await _digitalDisplayService
                            .RemoveDigitalDisplayAssetSetAsync(update.AssetId, update.SetId);
                        update.Success = true;
                    }
                }
                catch (OcudaException oex)
                {
                    update.Success = false;
                    update.Message = $"There was an error updating the asset set: {oex.Message}";
                }
            }
            return Json(update);
        }

        [HttpGet]
        [Route("[action]/{displayId}")]
        [SaveModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> UpdateDisplay(int displayId)
        {
            return View(new UpdateDisplayViewModel
            {
                DigitalDisplay = await _digitalDisplayService.GetDisplayAsync(displayId),
                DigitalDisplaySets = SetDropDown(await _digitalDisplayService.GetSetsAsync()),
                Locations = LocationDropDown(await _locationService.GetAllLocationsAsync()),
            });
        }

        [HttpPost]
        [Route("[action]/{displayId}")]
        [RestoreModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> UpdateDisplay(UpdateDisplayViewModel viewmodel)
        {
            if (viewmodel == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                await _digitalDisplayService.UpdateDisplayAsync(viewmodel.DigitalDisplay);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(UpdateDisplay),
                new { displayId = viewmodel.DigitalDisplay.Id });
        }

        [HttpGet]
        [Route("[action]")]
        [SaveModelState]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> UpdateSet(int digitalDisplaySetId)
        {
            var digitalDisplaySet = await _digitalDisplayService.GetSetAsync(digitalDisplaySetId);
            if (digitalDisplaySet != null)
            {
                return View("AddUpdateSet", digitalDisplaySet);
            }
            else
            {
                return RedirectToAction(nameof(Sets));
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> UploadAsset(IFormFile assetFile)
        {
            if (!await HasContentManagementRightsAsync())
            {
                ShowAlertDanger("You do not have permission to add an asset.");
                return RedirectToAction(nameof(Assets));
            }

            if (assetFile == null)
            {
                return RedirectToAction(nameof(UploadAsset));
            }

            await UploadAssetInternalAsync(assetFile);

            return RedirectToAction(nameof(Assets));
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Any error should return a valid JSON response")]
        public async Task<IActionResult> UploadJob(SlideUploadJob job)
        {
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
                return Json(ErrorJobResult("No job specified."));
            }

            var set = await _digitalDisplayService.GetSetAsync(job.Set.Trim());

            if (set == null)
            {
                return Json(ErrorJobResult($"Could not find set: {job.Set}"));
            }

            DigitalDisplayAsset asset;

            try
            {
                asset = await UploadAssetInternalAsync(job.File);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unable to add digital display asset for {Username}: {ErrorMessage}",
                    CurrentUsername,
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
                    CurrentUsername,
                    ex.Message);
                return Json(ErrorJobResult($"Error adding digital display asset: {ex.Message}"));
            }

            _logger.LogInformation("Added display asset for {Username}, asset {Asset} to set {Set}",
                CurrentUsername,
                asset.Name,
                set.Name);

            return Json(new JsonResponse
            {
                Url = Url.Action(nameof(AssetAssociations), new
                {
                    digitalDisplayAssetId = asset.Id
                }),
                Message = $"Added asset id {asset.Id} to set id {set.Id}",
                ServerResponse = true,
                Success = true
            });
        }

        private static JsonResponse ErrorJobResult(string message)
        {
            return new JsonResponse
            {
                Message = message,
                ServerResponse = true,
                Success = false
            };
        }

        private static IEnumerable<SelectListItem> LocationDropDown(List<Location> locations)
        {
            var locationList = locations.ConvertAll(_ =>
                new SelectListItem(_.Name, _.Id.ToString(CultureInfo.InvariantCulture)));
            locationList.Insert(0, new SelectListItem("", null));
            return locationList;
        }

        private static IEnumerable<SelectListItem> SetDropDown(ICollection<DigitalDisplaySet> sets)
        {
            return sets.ToList().ConvertAll(_ =>
                new SelectListItem(_.Name, _.Id.ToString(CultureInfo.InvariantCulture)));
        }

        private async Task<bool> HasContentManagementRightsAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.DigitalDisplayContentManagement);
        }

        private async Task<DigitalDisplayAsset> UploadAssetInternalAsync(IFormFile assetFile)
        {
            var fullFilePath = _digitalDisplayService.GetAssetPath(assetFile.FileName);

            int renameCounter = 1;
            while (System.IO.File.Exists(fullFilePath))
            {
                fullFilePath = _digitalDisplayService.GetAssetPath(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}-{1}{2}",
                    Path.GetFileNameWithoutExtension(assetFile.FileName),
                    renameCounter++,
                    Path.GetExtension(assetFile.FileName)));
            }

            using var fileStream = new FileStream(fullFilePath, FileMode.Create);
            await assetFile.CopyToAsync(fileStream);

            return await _digitalDisplayService.AddAssetAsync(new DigitalDisplayAsset
            {
                Name = assetFile.FileName,
                Path = Path.GetFileName(fullFilePath)
            });
        }
    }
}