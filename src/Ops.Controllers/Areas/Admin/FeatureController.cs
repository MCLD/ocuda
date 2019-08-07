using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Feature;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;
using Ocuda.Utility.TagHelpers;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class FeatureController : BaseController<FeatureController>
    {
        private readonly IFeatureService _featureService;
        private readonly IConfiguration _config;
        private readonly ILogger<HomeController> _logger;

        public FeatureController(IConfiguration config,
            ServiceFacades.Controller<FeatureController> context,
            IFeatureService featureService,
            ILogger<HomeController> logger) : base(context)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
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

        [Route("AddFeature")]
        [Route("{featureName}")]
        public async Task<IActionResult> Feature(string featureName)
        {
            if (string.IsNullOrEmpty(featureName))
            {
                var feature = new Feature();
                feature.IsNewFeature = true;
                var viewModel = new FeatureViewModel
                {
                    Feature = feature,
                };

                return View("FeatureDetails", viewModel);
            }
            else
            {
                try
                {
                    var feature = await _featureService.GetFeatureByNameAsync(featureName);
                    feature.IsNewFeature = false;
                    var viewModel = new FeatureViewModel
                    {
                        Feature = feature,
                    };
                    return View("FeatureDetails", viewModel);
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Feature does not exist: {ex.Message}");
                    _logger.LogError(ex.Message);
                    return RedirectToAction("Index");
                }
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateFeature(Feature feature)
        {
            if (string.IsNullOrEmpty(feature.Stub))
            {
                feature.Stub = "";
            }
            if (string.IsNullOrEmpty(feature.BodyText))
            {
                feature.BodyText = "";
            }
            if (feature.FontAwesome.Contains("fab"))
            {
                feature.FontAwesome = "fa-inverse " + feature.FontAwesome +" fa-stack-1x";
            }
            else
            {
                feature.FontAwesome = "fa fa-inverse " + feature.FontAwesome + " fa-stack-1x";
            }
            
            try
            {
                await _featureService.AddFeatureAsync(feature);
                ShowAlertSuccess($"Added Feature: {feature.Name}");
                feature.IsNewFeature = true;
                return RedirectToAction("Feature", new { featureName = feature.Name });
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Create Feature: {ex.Message}");
                _logger.LogError(ex.Message);
                feature.IsNewFeature = true;
                var viewModel = new FeatureViewModel
                {
                    Feature = feature,
                };

                return View("FeatureDetails", viewModel);
            }
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteFeature(Feature feature)
        {
            try
            {
                await _featureService.DeleteAsync(feature.Id);
                ShowAlertSuccess($"Deleted Feature: {feature.Name}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex.Message);
                ShowAlertDanger($"Unable to Delete Feature {feature.Name}: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditFeature(Feature feature)
        {
            if (string.IsNullOrEmpty(feature.Stub))
            {
                feature.Stub = "";
            }
            if (string.IsNullOrEmpty(feature.BodyText))
            {
                feature.BodyText = "";
            }
            if (feature.FontAwesome.Contains("fab"))
            {
                feature.FontAwesome = "fa-inverse " + feature.FontAwesome + " fa-stack-1x";
            }
            else
            {
                feature.FontAwesome = "fa fa-inverse " + feature.FontAwesome + " fa-stack-1x";
            }
            try
            {
                await _featureService.EditAsync(feature);
                ShowAlertSuccess($"Updated Feature: {feature.Name}");
                feature.IsNewFeature = false;
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Update Feature: {feature.Name}");
                _logger.LogError(ex.Message);
                feature.IsNewFeature = false;
                var viewModel = new FeatureViewModel
                {
                    Feature = feature,
                };

                return View("FeatureDetails", viewModel);
            }
            return RedirectToAction("Feature", new { featureName = feature.Name });
        }
    }
}