﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Podcasts;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class PodcastsController : BaseController<PodcastsController>
    {
        private const long MaximumFileSizeBytes = 75 * 1024 * 1024;

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IPodcastService _podcastService;

        public static string Name { get { return "Podcasts"; } }
        public static string Area { get { return "SiteManagement"; } }

        public PodcastsController(ServiceFacades.Controller<PodcastsController> context,
            IDateTimeProvider dateTimeProvider,
            IPermissionGroupService permissionGroupService,
            IPodcastService podcastService)
            : base(context)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _podcastService = podcastService
                ?? throw new ArgumentNullException(nameof(podcastService));
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

            var podcastList = await _podcastService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = podcastList.Count,
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

            var viewModel = new IndexViewModel
            {
                Podcasts = podcastList.Data,
                PaginateModel = paginateModel,
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                PermissionIds = UserClaims(ClaimType.PermissionId)
            };

            return View(viewModel);
        }

        [Route("[action]/{podcastId}")]
        public async Task<IActionResult> Details(int podcastId)
        {
            var promenadeUrl = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

            return View(new DetailsViewModel
            {
                Podcast = await _podcastService.GetByIdAsync(podcastId),
                PromenadeUrl = promenadeUrl.Trim('/')
            });
        }

        [Route("[action]/{podcastId}")]
        [Route("[action]/{podcastId}/{page}")]
        public async Task<IActionResult> Episodes(int podcastId, int page = 1)
        {
            if (!await HasPermissionAsync<PermissionGroupPodcastItem>(_permissionGroupService,
                podcastId))
            {
                return RedirectToUnauthorized();
            }

            var filter = new BaseFilter(page);

            var episodes = await _podcastService.GetPaginatedEpisodeListAsync(podcastId, filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = episodes.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = paginateModel.LastPage ?? 1 });
            }

            var podcast = await _podcastService.GetByIdAsync(podcastId);

            var viewModel = new EpisodeIndexViewModel
            {
                PodcastId = podcastId,
                PodcastTitle = podcast.Title,
                Episodes = episodes.Data,
                PaginateModel = paginateModel,
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                HasPermission = UserClaims(ClaimType.PermissionId).Any(_ => podcast.PermissionGroupIds?.Contains(_) == true)
            };

            return View(viewModel);
        }

        [Route("[action]/{podcastId}")]
        public async Task<IActionResult> AddEpisode(int podcastId)
        {
            if (!await HasPermissionAsync<PermissionGroupPodcastItem>(_permissionGroupService,
                podcastId))
            {
                return RedirectToUnauthorized();
            }

            var podcast = await _podcastService.GetByIdAsync(podcastId);

            return View("EpisodeDetails", new EpisodeDetailsViewModel
            {
                Episode = new PodcastItem
                {
                    PodcastId = podcastId,
                    Episode = podcast.EpisodeCount + 1
                },
                PodcastTitle = podcast.Title,
                MaximumFileSizeMB = $"{MaximumFileSizeBytes / 1024 / 1024}"
            });
        }

        [Route("[action]/{episodeId}")]
        public async Task<IActionResult> EditEpisode(int episodeId)
        {
            var episode = await _podcastService.GetPodcastItemByIdAsync(episodeId);

            if (!await HasPermissionAsync<PermissionGroupPodcastItem>(_permissionGroupService,
                episode.PodcastId))
            {
                return RedirectToUnauthorized();
            }

            var podcast = await _podcastService.GetByIdAsync(episode.PodcastId);

            var viewModel = new EpisodeDetailsViewModel
            {
                EditEpisode = true,
                Episode = episode,
                PodcastTitle = episode.Podcast.Title,
                MaximumFileSizeMB = $"{MaximumFileSizeBytes / 1024 / 1024}"
            };

            if (!string.IsNullOrEmpty(podcast.Stub)
                && episode.Episode != null)
            {
                var path = await FilePathToEpisodeAsync(podcast.Stub, episode.Episode);
                var filename = FilenameOfEpisode(podcast.Stub, episode.Episode);

                if (!string.IsNullOrEmpty(path))
                {
                    if (System.IO.File.Exists(Path.Combine(path, filename)))
                    {
                        viewModel.UploadedAt = System.IO.File.GetLastWriteTime(path);
                        viewModel.Filename = filename;
                    }
                    else
                    {
                        viewModel.Filename = filename;
                        viewModel.FileMissing = true;
                    }
                }
            }

            return View("EpisodeDetails", viewModel);
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> AddUpdateEpisode(EpisodeDetailsViewModel viewModel)
        {
            if (viewModel == null)
            {
                AlertWarning = "No file selected.";
                return RedirectToAction(nameof(Index));
            }

            if (!await HasPermissionAsync<PermissionGroupPodcastItem>(_permissionGroupService,
                viewModel.Episode.PodcastId))
            {
                return RedirectToUnauthorized();
            }

            if (viewModel.EditEpisode)
            {
                var currentEpisode
                    = await _podcastService.GetPodcastItemByIdAsync(viewModel.Episode.Id);

                if (currentEpisode.Episode != viewModel.Episode.Episode)
                {
                    ModelState.AddModelError("Episode.Episode", "Please contact an administrator to change episode numbers.");
                    return View("EpisodeDetails", viewModel);
                }
                else
                {
                    currentEpisode.Description = viewModel.Episode.Description.Trim();
                    if (!string.IsNullOrEmpty(viewModel.Episode.Keywords))
                    {
                        currentEpisode.Keywords = viewModel.Episode.Keywords
                            .Replace(", ", ",", StringComparison.OrdinalIgnoreCase)
                            .Trim();
                    }
                    currentEpisode.PublishDate = viewModel.Episode.PublishDate;
                    currentEpisode.Subtitle = viewModel.Episode.Subtitle;
                    currentEpisode.Title = viewModel.Episode.Title;
                    currentEpisode.UpdatedAt = DateTime.Now;

                    // save currentepisode
                    await _podcastService.UpdatePodcastItemAsync(currentEpisode);

                    AlertSuccess = $"Updated episode #{viewModel.Episode.Episode}: {viewModel.Episode.Title}.";

                    return RedirectToAction(nameof(EditEpisode),
                        new { episodeId = viewModel.Episode.Id });
                }
            }
            else
            {
                // insert new episode

                if (viewModel.Episode?.Episode == null || viewModel.Episode.Episode == 0)
                {
                    ModelState.Clear();
                    ModelState.AddModelError("Episode.Episode", "You must supply an episode number.");
                    return View("EpisodeDetails", viewModel);
                }

                if (await _podcastService.HasEpisodeAsync(viewModel.Episode.PodcastId,
                    (int)viewModel.Episode.Episode))
                {
                    ModelState.Clear();
                    ModelState.AddModelError("Episode.Episode", $"This podcast already has an episode #{viewModel.Episode.Episode}.");
                    return View("EpisodeDetails", viewModel);
                }

                string baseUrl = await _siteSettingService
                        .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

                var podcast = await _podcastService.GetByIdAsync(viewModel.Episode.PodcastId);

                viewModel.Episode.Stub = $"ep-{viewModel.Episode.Episode}";
                viewModel.Episode.MediaUrl = string.Format(CultureInfo.InvariantCulture,
                    "/podcasts/{0}/ep-{1}/{0}-ep-{1}.mp3",
                    podcast.Stub,
                    viewModel.Episode.Episode);
                viewModel.Episode.Guid = string.Format(CultureInfo.InvariantCulture,
                    "{0}/podcasts/{1}/ep-{2}/",
                    baseUrl.Trim('/'),
                    podcast.Stub,
                    viewModel.Episode.Episode);
                viewModel.Episode.GuidPermaLink = true;
                viewModel.Episode.MediaType = "audio/mpeg";
                viewModel.Episode.Season = 1;

                ModelState.Clear();

                if (TryValidateModel(viewModel.Episode, nameof(PodcastItem)))
                {
                    viewModel.Episode.UpdatedAt = DateTime.Now;
                    await _podcastService.AddPodcastItemAsync(viewModel.Episode);

                    AlertSuccess = $"Added episode #{viewModel.Episode.Episode}: {viewModel.Episode.Title}.";

                    return RedirectToAction(nameof(EditEpisode),
                        new { episodeId = viewModel.Episode.Id });
                }
                else
                {
                    var issues = ModelState
                        .Values
                        .Where(_ => _.ValidationState != ModelValidationState.Valid);

                    if (issues.Any())
                    {
                        var sb = new StringBuilder("Please correct the following issues:<ul>");
                        foreach (var issue in issues)
                        {
                            for (var i = 0; i < issue.Errors.Count; i++)
                            {
                                sb.Append("<li class=\"mb-0\">")
                                    .Append(issue.Errors[i].ErrorMessage)
                                    .Append("</li>");
                            }
                        }
                        sb.Append("</ul>");
                        AlertWarning = sb.ToString();
                    }

                    return View("EpisodeDetails", viewModel);
                }
            }
        }

        private async Task<string> FilePathToEpisodeAsync(string podcastStub, int? episodeNumber)
        {
            if (episodeNumber == null)
            {
                throw new ArgumentNullException(nameof(episodeNumber));
            }

            string basePath = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);
            return Path.Combine(basePath, "podcasts",
                podcastStub,
                $"ep-{episodeNumber}");
        }

        private string FilenameOfEpisode(string podcastStub, int? episodeNumber)
        {
            if (episodeNumber == null)
            {
                throw new ArgumentNullException(nameof(episodeNumber));
            }

            return $"{podcastStub}-ep-{episodeNumber}.mp3";
        }

        [HttpPost]
        [Route("[action]")]
        [RequestSizeLimit(MaximumFileSizeBytes)]
        public async Task<IActionResult> UpdatePodcastFile(EpisodeDetailsViewModel viewModel)
        {
            if (viewModel?.UploadedFile == null)
            {
                return RedirectToAction(nameof(EditEpisode),
                    new { episodeId = viewModel?.Episode?.Id });
            }

            var podcastItem = await _podcastService.GetPodcastItemByIdAsync(viewModel.Episode.Id);
            var podcast = await _podcastService.GetByIdAsync(podcastItem.PodcastId);

            if (!await HasPermissionAsync<PermissionGroupPodcastItem>(_permissionGroupService,
                podcast.Id))
            {
                return RedirectToUnauthorized();
            }

            var path = await FilePathToEpisodeAsync(podcast.Stub, podcastItem.Episode);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, FilenameOfEpisode(podcast.Stub, podcastItem.Episode));

            ModelState.Clear();

            if (viewModel.UploadedFile.ContentType == "audio/mpeg")
            {
                try
                {
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    using var outputFile = new FileStream(path, FileMode.CreateNew);
                    await viewModel.UploadedFile.CopyToAsync(outputFile);
                    outputFile.Close();

                    PodcastItem podcastFileInfo = _podcastService.GetFileInfo(path);

                    podcastItem.MediaType = viewModel.UploadedFile.ContentType;
                    podcastItem.Duration = podcastFileInfo.Duration;
                    podcastItem.MediaSize = podcastFileInfo.MediaSize;
                    podcastItem.UpdatedAt = _dateTimeProvider.Now;
                    await _podcastService.UpdatePodcastItemAsync(podcastItem);

                    AlertSuccess = "Podcast file updated successfully.";

                    return RedirectToAction(nameof(EditEpisode),
                        new { episodeId = viewModel.Episode.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(nameof(EpisodeDetailsViewModel.UploadedFile),
                        $"Unable to save file: {ex.Message}");

                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    podcastItem.MediaType = string.Empty;
                    podcastItem.Duration = 0;
                    podcastItem.MediaSize = 0;
                    podcastItem.UpdatedAt = _dateTimeProvider.Now;
                    await _podcastService.UpdatePodcastItemAsync(podcastItem);
                }
            }
            else
            {
                ModelState.AddModelError(nameof(EpisodeDetailsViewModel.UploadedFile),
                    "Please upload an .mp3 file.");
            }

            return View("EpisodeDetails", viewModel);
        }

        [Route("[action]/{podcastId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Permissions(int podcastId)
        {
            var podcast = await _podcastService.GetByIdAsync(podcastId);

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var podcastPermissions = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupPodcastItem>(podcastId);

            var availableGroups = new Dictionary<int, string>();
            var assignedGroups = new Dictionary<int, string>();

            foreach (var permissionGroup in permissionGroups)
            {
                var permission = podcastPermissions
                    .SingleOrDefault(_ => _.PermissionGroupId == permissionGroup.Id);
                if (permission == null)
                {
                    availableGroups.Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
                else
                {
                    assignedGroups.Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
            }

            return View(new EpisodePermissionsViewModel
            {
                Title = podcast.Title,
                Stub = podcast.Stub,
                PodcastId = podcast.Id,
                AvailableGroups = availableGroups,
                AssignedGroups = assignedGroups
            });
        }

        [HttpPost]
        [Route("[action]/{podcastId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddPermissionGroup(int podcastId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupPodcastItem>(podcastId,
                    permissionGroupId);
                AlertInfo = "Content permission added.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { podcastId });
        }

        [HttpPost]
        [Route("[action]/{podcastId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemovePermissionGroup(int podcastId,
            int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupPodcastItem>(podcastId,
                    permissionGroupId);
                AlertInfo = "Content permission removed.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem removing permission: {ex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { podcastId });
        }
    }
}
