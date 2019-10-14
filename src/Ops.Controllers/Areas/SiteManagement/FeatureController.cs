using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Feature;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class FeaturesController : BaseController<FeaturesController>
    {
        private readonly IFeatureService _featureService;

        public static string Name { get { return "Features"; } }

        public FeaturesController(ServiceFacades.Controller<FeaturesController> context,
            IFeatureService featureService) : base(context)
        {
            _featureService = featureService
                ?? throw new ArgumentNullException(nameof(featureService));
        }

        [Route("")]
        [Route("[action]")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var featureList = await _featureService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = featureList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new FeatureViewModel
            {
                AllFeatures = featureList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [Route("CreateFeature")]
        [RestoreModelState]
        public IActionResult CreateFeature()
        {
            return View("FeatureDetails", new FeatureViewModel
            {
                Feature = new Feature
                {
                    IsNewFeature = true
                },
                Action = nameof(FeaturesController.CreateFeature)
            });
        }

        [Route("{featureName}")]
        [RestoreModelState]
        public async Task<IActionResult> Feature(string featureName)
        {
            var feature = await _featureService.GetFeatureByNameAsync(featureName);
            if (feature != null)
            {
                feature.IsNewFeature = false;
                return View("FeatureDetails", new FeatureViewModel
                {
                    Feature = feature,
                    Action = nameof(FeaturesController.EditFeature)
                });
            }
            else
            {
                ShowAlertDanger("Feature does not exist.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateFeature(Feature feature)
        {
            if(feature.NeedsPopup && string.IsNullOrEmpty(feature.Stub))
            {
                ModelState.AddModelError("Feature.Stub", "A 'Stub' is required for a popup.");
                ShowAlertDanger($"Invalid paramaters");
                feature.IsNewFeature = true;
                return RedirectToAction(nameof(CreateFeature));
            }
            if (ModelState.IsValid)
            {
                try
                {
                    await _featureService.AddFeatureAsync(feature);
                    ShowAlertSuccess($"Added Feature: {feature.Name}");
                    feature.IsNewFeature = true;
                    return RedirectToAction(nameof(Feature), new { featureName = feature.Name.ToLower().Replace(" ", "") });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Create Feature: {ex.Message}");
                    _logger.LogError(ex, "Problem creating feature: {Message}", ex.Message);
                    feature.IsNewFeature = true;
                    return RedirectToAction(nameof(Feature));
                }
            }
            else
            {
                ShowAlertDanger($"Invalid paramaters");
                feature.IsNewFeature = true;
                return RedirectToAction(nameof(CreateFeature));
            }
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteFeature(Feature feature)
        {
            try
            {
                await _featureService.DeleteAsync(feature.Id);
                ShowAlertSuccess($"Deleted Feature: {feature.Name}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Problem deleting feature: {Message}", ex.Message);
                ShowAlertDanger($"Unable to Delete Feature {feature.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditFeature(Feature feature)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _featureService.EditAsync(feature);
                    ShowAlertSuccess($"Updated Feature: {feature.Name}");
                    return RedirectToAction(nameof(Feature), new { featureName = feature.Name.ToLower().Replace(" ", "") });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update Feature: {feature.Name}");
                    _logger.LogError(ex, "Problem updating feature: {Message}", ex.Message);
                    feature.IsNewFeature = false;
                    return RedirectToAction(nameof(Feature), new { featureName = feature.Name.ToLower().Replace(" ", "") });
                }
            }
            else
            {
                ShowAlertDanger($"Invalid Parameters: {feature.Name}");
                feature.IsNewFeature = false;
                return RedirectToAction(nameof(Feature), new { featureName = feature.Name.ToLower().Replace(" ", "") });
            }
        }
    }
}