using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class SegmentsController : BaseController<SegmentsController>
    {
        private readonly IEmediaService _emediaService;
        private readonly IFeatureService _featureService;
        private readonly IVolunteerFormService _formService;
        private readonly ILanguageService _languageService;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IPodcastService _podcastService;
        private readonly IProductService _productService;
        private readonly ISegmentService _segmentService;
        private readonly ISegmentWrapService _segmentWrapService;

        public SegmentsController(ServiceFacades.Controller<SegmentsController> context,
            IEmediaService emediaService,
            IFeatureService featureService,
            ILanguageService languageService,
            ILocationFeatureService locationFeatureService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            IPodcastService podcastService,
            IProductService productService,
            ISegmentService segmentService,
            ISegmentWrapService segmentWrapService,
            IVolunteerFormService formService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(emediaService);
            ArgumentNullException.ThrowIfNull(featureService);
            ArgumentNullException.ThrowIfNull(formService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(locationFeatureService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(podcastService);
            ArgumentNullException.ThrowIfNull(productService);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _emediaService = emediaService;
            _featureService = featureService;
            _formService = formService;
            _languageService = languageService;
            _locationFeatureService = locationFeatureService;
            _locationService = locationService;
            _permissionGroupService = permissionGroupService;
            _podcastService = podcastService;
            _productService = productService;
            _segmentService = segmentService;
            _segmentWrapService = segmentWrapService;
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Segments"; } }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            if (model == null)
            {
                return Json(new JsonResponse
                {
                    Success = false,
                    Message = "Invalid request to create a segment."
                });
            }

            if (model.SegmentStartDate.HasValue && model.SegmentStartTime.HasValue)
            {
                model.Segment.StartDate = model
                    .SegmentStartDate.Value.CombineWithTime(model.SegmentStartTime.Value);
            }

            if (model.SegmentEndDate.HasValue && model.SegmentEndTime.HasValue)
            {
                model.Segment.EndDate = model
                    .SegmentEndDate.Value.CombineWithTime(model.SegmentStartTime.Value);
            }

            if (model.Segment.StartDate.HasValue && model.Segment.EndDate.HasValue
                && model.Segment.StartDate > model.Segment.EndDate)
            {
                ModelState.AddModelError("Segment.StartDate",
                    "Start Date cannot be after the End Date.");
            }

            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var segment = await _segmentService.CreateAsync(model.Segment);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Detail), new { id = segment.Id })
                    };

                    ShowAlertSuccess($"Created segment: {segment.Name}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            try
            {
                await _segmentService.DeleteAsync(model.Segment.Id);
                ShowAlertSuccess($"Deleted segment: {model.Segment.Name}");
            }
            catch (OcudaException ex)
            {
                var alertMessage = $"Unable to delete segment \"{model.Segment.Name}\": {ex.Message}";

                var inUseByList = (List<string>)ex.Data[OcudaExceptionData.SegmentInUseBy];
                if (inUseByList != null)
                {
                    var inUseByTag = new TagBuilder("ul");
                    foreach (var usedBy in inUseByList)
                    {
                        var inUseByItem = new TagBuilder("li");
                        inUseByItem.InnerHtml.SetContent(usedBy);
                        inUseByTag.InnerHtml.AppendHtml(inUseByItem);
                    }

                    await using var inUseByHtml = new StringWriter();
                    inUseByTag.WriteTo(inUseByHtml, HtmlEncoder.Default);
                    ShowAlertDanger($"{alertMessage} {inUseByHtml}");
                }
                else
                {
                    ShowAlertDanger(alertMessage);
                }
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteText(DetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasSegmentPermissionAsync(model.SegmentId))
            {
                return RedirectToUnauthorized();
            }

            var segmentText = await _segmentService.GetBySegmentAndLanguageAsync(model.SegmentId,
                model.LanguageId);

            await _segmentService.DeleteSegmentTextAsync(segmentText);

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            ShowAlertSuccess($"Deleted Segment {language.Description} text!");

            return RedirectToAction(nameof(Detail),
                new
                {
                    id = model.SegmentId,
                    language = language.Name
                });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Detail(int id, string language)
        {
            if (!await HasSegmentPermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var segment = await _segmentService.GetByIdAsync(id);
            if (segment == null)
            {
                ShowAlertDanger($"Could not find Segment with ID: {id}");
                return RedirectToAction(nameof(SegmentsController.Index));
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var segmentText = await _segmentService
                .GetBySegmentAndLanguageAsync(id, selectedLanguage.Id);

            var wrapList = await _segmentWrapService.GetActiveListAsync();
            if (wrapList?.Count > 0)
            {
                wrapList.Add("", "No wrap");
            }

            var viewModel = new DetailViewModel
            {
                LanguageDescription = selectedLanguage.Description,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                SegmentEndDate = segment.EndDate,
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                SegmentStartDate = segment.StartDate,
                SegmentText = await _segmentService
                    .GetBySegmentAndLanguageAsync(id, selectedLanguage.Id),
                SegmentWrapId = segment.SegmentWrapId,
                SegmentWrapList = new SelectList(wrapList.OrderBy(_ => _.Key),
                    "Key",
                    "Value",
                    segment.SegmentWrapId)
            };

            viewModel.NewSegmentText = viewModel.SegmentText == null;

            // check if this segment is used elsewhere so we can contextualize the back button

            var pageLayoutId
                = await _segmentService.GetPageLayoutIdForSegmentAsync(segment.Id);

            if (pageLayoutId.HasValue)
            {
                viewModel.BackLink = Url.Action(nameof(PagesController.LayoutDetail),
                    PagesController.Name,
                    new
                    {
                        id = pageLayoutId.Value
                    });
                viewModel.Relationship
                    = $"This segment is used page layout ID: {pageLayoutId.Value}";
            }
            else
            {
                await EstablishBacklinkAsync(segment.Id, viewModel);
            }

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Detail(DetailViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasSegmentPermissionAsync(model.SegmentId))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            if (ModelState.IsValid)
            {
                var segment = await _segmentService.GetByIdAsync(model.SegmentId);
                if (segment != null && model.SegmentWrapId != segment.SegmentWrapId)
                {
                    await _segmentService.UpdateWrapAsync(segment.Id, model.SegmentWrapId);
                }

                var segmentText = model.SegmentText;
                segmentText.LanguageId = language.Id;
                segmentText.SegmentId = model.SegmentId;

                var currentSegmentText = await _segmentService.GetBySegmentAndLanguageAsync(
                    model.SegmentId, language.Id);

                if (currentSegmentText == null)
                {
                    await _segmentService.CreateSegmentTextAsync(segmentText);

                    ShowAlertSuccess("Added segment text!");
                }
                else
                {
                    await _segmentService.EditSegmentTextAsync(segmentText);

                    ShowAlertSuccess("Updated segment text!");
                }

                // if this was an update to the name of a feature then update the name item as well
                var defaultLanguage = await _languageService.GetDefaultLanguageId();
                if (language.Id == defaultLanguage)
                {
                    var feature = await _featureService.GetFeatureBySegmentIdAsync(segment.Id);
                    if (feature?.NameSegmentId == segment.Id)
                    {
                        await _featureService.UpdateFeatureNameAsync(feature.Id, segmentText.Text);
                    }
                }
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.SegmentId,
                language = language.Name
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> Edit(IndexViewModel model)
        {
            if (model == null)
            {
                return Json(new JsonResponse
                {
                    Success = false,
                    Message = "Invalid request to update a segment."
                });
            }

            if (model.SegmentStartDate.HasValue && model.SegmentStartTime.HasValue)
            {
                model.Segment.StartDate = model
                    .SegmentStartDate.Value.CombineWithTime(model.SegmentStartTime.Value);
            }

            if (model.SegmentEndDate.HasValue && model.SegmentEndTime.HasValue)
            {
                model.Segment.EndDate = model
                    .SegmentEndDate.Value.CombineWithTime(model.SegmentStartTime.Value);
            }

            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var segment = await _segmentService.EditAsync(model.Segment);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                    ShowAlertSuccess($"Updated segment: {segment.Name}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);
            var segmentList = await _segmentService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = segmentList.Count,
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

            foreach (var segment in segmentList.Data.ToList())
            {
                segment.SegmentLanguages = await _segmentService.GetSegmentLanguagesByIdAsync(segment.Id);
            }
            var languages = await _languageService.GetActiveAsync();
            var selectedLanguage = languages.Single(_ => _.IsDefault);
            var viewModel = new IndexViewModel
            {
                Segments = segmentList.Data,
                PaginateModel = paginateModel,
                LanguageList = new SelectList(languages, nameof(Language.Id),
                    nameof(Language.Description), selectedLanguage.Id),
                AvailableLanguages = languages.Select(_ => _.Name).ToList()
            };
            return View(viewModel);
        }

        private async Task EstablishBacklinkAsync(int segmentId,
                                            DetailViewModel viewModel)
        {
            var emediaGroup = await _emediaService.GetGroupUsingSegmentAsync(segmentId);

            if (emediaGroup != null)
            {
                viewModel.BackLink = Url.Action(nameof(EmediaController.GroupDetails),
                    EmediaController.Name,
                    new
                    {
                        id = emediaGroup.Id
                    });
                viewModel.Relationship
                    = $"This segment is used by emedia group: {emediaGroup.Name}";
                return;
            }

            var feature = await _featureService.GetFeatureBySegmentIdAsync(segmentId);
            if (feature != null)
            {
                viewModel.BackLink = Url.Action(nameof(FeaturesController.Feature),
                    FeaturesController.Name,
                    new
                    {
                        area = FeaturesController.Area,
                        slug = feature.Stub
                    });
                viewModel.Relationship = $"This segment is used for feature: {feature.Name}";
            }

            var locations = await _locationService.GetLocationsBySegment(segmentId);

            if (locations?.Count == 1)
            {
                viewModel.BackLink = Url.Action(nameof(Controllers.LocationsController.Details),
                    Controllers.LocationsController.Name,
                    new
                    {
                        area = "",
                        slug = locations.First().Stub
                    });
                viewModel.Relationship
                    = $"This segment is used for location: {locations.First().Name}";
                return;
            }
            if (locations?.Count > 1)
            {
                viewModel.Relationship = string.Format(CultureInfo.InvariantCulture,
                    "This segment is used for multiple locations: {0}",
                    string.Join(", ", locations.Select(_ => _.Name)));
                return;
            }

            var locationFeature = await _locationFeatureService.GetLocationFeatureBySegmentIdAsync(segmentId);
            if (locationFeature != null)
            {
                var location = await _locationService.GetLocationByIdAsync(locationFeature.LocationId);
                viewModel.BackLink = Url.Action(nameof(Controllers.LocationsController.LocationFeature),
                    Controllers.LocationsController.Name,
                    new
                    {
                        area = "",
                        slug = location.Stub,
                        featureId = locationFeature.FeatureId
                    });
                viewModel.Relationship = "This segment is used to customize a location feature description.";
                return;
            }

            var episode = await _podcastService.GetEpisodeBySegmentIdAsync(segmentId);

            if (episode != null)
            {
                viewModel.BackLink = Url.Action(nameof(PodcastsController.EditEpisode),
                    PodcastsController.Name,
                    new
                    {
                        episodeId = episode.Id
                    });
                viewModel.Relationship
                    = $"This segment is used for podcast '{episode.Podcast.Title}' episode #{episode.Episode.Value}";
                string published = episode.PublishDate.HasValue
                    ? $"published {episode.PublishDate.Value.ToLongDateString()}"
                    : "not yet published";
                viewModel.AutomatedHeaderMarkup
                    = $"<strong>Show notes for {episode.Title}</strong><br>{episode.Podcast.Title}. <em>Episode {episode.Episode}, {published}.</em>";
                viewModel.IsShowNotes = episode != null;
                return;
            }

            var forms = await _formService.GetFormBySegmentIdAsync(segmentId);
            if (forms?.Count == 1)
            {
                viewModel.BackLink = Url.Action(nameof(VolunteerController.Form),
                    VolunteerController.Name,
                    new
                    {
                        id = forms.First().Id
                    });
                return;
            }

            var products = await _productService.GetBySegmentIdAsync(segmentId);

            if (products?.Count == 1)
            {
                viewModel.BackLink = Url.Action(nameof(ProductsController.Details),
                    ProductsController.Name,
                    new
                    {
                        productSlug = products.First().Slug
                    });
                viewModel.Relationship
                    = $"This segment is used for product: {products.First().Name}";
                viewModel.AutomatedHeaderMarkup
                    = $"<strong>{products.First().Name}</strong>";
            }
            else if (products?.Count > 1)
            {
                viewModel.Relationship = string.Format(CultureInfo.InvariantCulture,
                    "This segment is used for multiple products: {0}",
                    string.Join(", ", products.Select(_ => _.Name)));
            }
        }

        private async Task<bool> HasSegmentPermissionAsync(int segmentId)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    var pageHeaderId = await _segmentService.GetPageHeaderIdForSegmentAsync(
                        segmentId);
                    if (pageHeaderId.HasValue)
                    {
                        var permissionGroups = await _permissionGroupService
                            .GetPermissionsAsync<PermissionGroupPageContent>(pageHeaderId.Value);
                        var permissionGroupsStrings = permissionGroups
                            .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                        return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                    }

                    var emediaGroup = await _emediaService.GetGroupUsingSegmentAsync(segmentId);
                    if (emediaGroup != null)
                    {
                        return await HasAppPermissionAsync(_permissionGroupService,
                            ApplicationPermission.EmediaManagement);
                    }

                    var podcast = await _podcastService.GetEpisodeBySegmentIdAsync(segmentId);
                    if (podcast != null)
                    {
                        return await HasPermissionAsync<PermissionGroupPodcastItem>(_permissionGroupService,
                            podcast.PodcastId) && await HasAppPermissionAsync(_permissionGroupService,
                                ApplicationPermission.PodcastShowNotesManagement);
                    }
                }
                return false;
            }
        }
    }
}