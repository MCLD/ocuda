using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.DigitalLibrary;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Promenade.Service.Filters;
using Ocuda.Utility.Extensions;

namespace Ocuda.Promenade.Controllers
{
    [Route("digital-library")]
    [Route("{culture:cultureConstraint?}/digital-library")]
    public class DigitalLibraryController(
        ServiceFacades.Controller<DigitalLibraryController> context,
        EmediaService emediaService,
        SocialCardService socialCardService,
        SubjectService subjectService) : BaseController<DigitalLibraryController>(context)
    {
        private readonly EmediaService _emediaService = emediaService
            ?? throw new ArgumentNullException(nameof(emediaService));

        private readonly SocialCardService _socialCardService = socialCardService
            ?? throw new ArgumentNullException(nameof(socialCardService));

        private readonly SubjectService _subjectService = subjectService
            ?? throw new ArgumentNullException(nameof(subjectService));

        public static void ApplyCommonMarkFormatting(IEnumerable<Emedia> emedias)
        {
            if (emedias?.Count() > 0)
            {
                foreach (var emedia in emedias)
                {
                    if (!string.IsNullOrWhiteSpace(emedia.EmediaText?.Description))
                    {
                        emedia.EmediaText.Description = CommonMark.CommonMarkConverter
                            .Convert(emedia.EmediaText.Description);
                    }
                    if (!string.IsNullOrWhiteSpace(emedia.EmediaText?.Details))
                    {
                        emedia.EmediaText.Details = CommonMark.CommonMarkConverter
                            .Convert(emedia.EmediaText?.Details);
                    }
                }
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> All()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var emediaViewModel = await GetViewModelAsync(forceReload);

            emediaViewModel.GroupedEmedia.Add(new EmediaGroup
            {
                Emedias = await _emediaService.GetEmediaAsync(forceReload),
                Segment = new Segment
                {
                    SegmentText = new SegmentText
                    {
                        Header = _localizer[i18n.Keys.Promenade.EmediaAll]
                    }
                },
                SortOrder = 1
            });

            foreach (var group in emediaViewModel.GroupedEmedia)
            {
                ApplyCommonMarkFormatting(group.Emedias);
            }

            PageTitle = "Digital Library";

            return View("Index", emediaViewModel);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var groupedEmedia = await _emediaService.GetGroupedEmediaAsync(forceReload);

            var firstGroup = true;

            foreach (var group in groupedEmedia)
            {
                ApplyCommonMarkFormatting(group.Emedias);

                if (!string.IsNullOrWhiteSpace(group.Segment?.SegmentText?.Text))
                {
                    group.Segment.SegmentText.Text = FormatForDisplay(group.Segment.SegmentText);
                }
                else if (firstGroup)
                {
                    // no segment text on first group, use button text
                    group.Segment = new Segment
                    {
                        SegmentText = new SegmentText
                        {
                            Header = _localizer[i18n.Keys.Promenade.EmediaGroups]
                        }
                    };
                }
                firstGroup = false;
            }

            var emediaViewModel = await GetViewModelAsync(forceReload, groupedEmedia);

            PageTitle = "Digital Library";

            return View(emediaViewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Launch(string id)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            if (!await ValidateRefererAsync(forceReload))
            {
                return RedirectToAction(nameof(Index));
            }

            var emedia = await _emediaService.GetAsync(forceReload, id);

            if (emedia == null)
            {
                return NotFound();
            }

            var isLocalNetwork = HttpContext.Items[ItemKey.IsLocalNetwork] as bool? == true;

            if (!emedia.IsAvailableExternally && !isLocalNetwork)
            {
                return View("NotAvailable");
            }

            if (emedia.IsHttpPost)
            {
                var launchViewModel = new LaunchViewModel
                {
                    Name = emedia.Name,
                    Uri = new Uri(emedia.RedirectUrl)
                };
                foreach (var (s, sv) in QueryHelpers.ParseQuery(launchViewModel.Uri.Query))
                {
                    launchViewModel.QueryStringValues.Add(s, sv);
                }
                return View(launchViewModel);
            }
            else
            {
                return Redirect(emedia.RedirectUrl);
            }
        }

        [HttpGet("[action]/{subject}")]
        public async Task<IActionResult> Subject(string subject)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var subjects = await _subjectService.GetAllAsync(forceReload);

            var selectedSubject = subjects.SingleOrDefault(_ => _.Slug == subject);

            if (selectedSubject == null)
            {
                return NotFound();
            }

            var filter = new EmediaFilter
            {
                SubjectId = selectedSubject.Id
            };

            var emediaViewModel = await GetViewModelAsync(forceReload);

            emediaViewModel.ActiveKey = subject;

            var subjectText = await _subjectService.GetTextAsync(forceReload, selectedSubject.Id);

            emediaViewModel.GroupedEmedia.Add(new EmediaGroup
            {
                SortOrder = 1,
                Emedias = await _emediaService.GetEmediaAsync(forceReload, filter),
                Segment = new Segment
                {
                    SegmentText = new SegmentText
                    {
                        Header = subjectText.Text
                    }
                }
            });

            foreach (var group in emediaViewModel.GroupedEmedia)
            {
                ApplyCommonMarkFormatting(group.Emedias);
            }

            PageTitle = "Digital Library";

            return View("Index", emediaViewModel);
        }

        private async Task<DigitalLibraryViewModel> GetViewModelAsync(bool forceReload)
        {
            return await GetViewModelAsync(forceReload, null);
        }

        private async Task<DigitalLibraryViewModel> GetViewModelAsync(bool forceReload,
            ICollection<EmediaGroup> emediaGroups)
        {
            var emediaSocial = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.Social.EmediaCardId, forceReload);

            var emediaViewModel = new DigitalLibraryViewModel
            {
                AllDescription = _localizer[i18n.Keys.Promenade.EmediaAll],
                IsLocalNetwork = HttpContext.Items[ItemKey.IsLocalNetwork] as bool? == true,
                PopularDescription = _localizer[i18n.Keys.Promenade.EmediaGroups],
                SocialCard = emediaSocial > -1
                    ? await _socialCardService.GetByIdAsync(emediaSocial, forceReload)
                    : null
            };

            emediaViewModel.SlugsSubjects
                .AddRange(await _subjectService.GetSlugsDescriptionsAsync(forceReload));

            if (emediaGroups?.Count > 0)
            {
                emediaViewModel.GroupedEmedia.AddRange(emediaGroups);
            }

            return emediaViewModel;
        }

        private async Task<bool> ValidateRefererAsync(bool forceReload)
        {
            var validReferers = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.Emedia.ValidReferers, forceReload);

            if (validReferers.Equals("*", StringComparison.Ordinal))
            {
                return true;
            }

            var referer = Request.Headers.Referer;

            if (string.IsNullOrWhiteSpace(referer)) { return false; }

            Uri refererUri;

            try
            {
                refererUri = new Uri(referer);
            }
            catch (UriFormatException)
            {
                return false;
            }

            if (refererUri == null) { return false; }

            if (string.IsNullOrWhiteSpace(validReferers))
            {
                _logger.LogError("No configured valid referers for launching electronic resources!");
            }

            return validReferers.Split(",").Contains(refererUri.Host);
        }
    }
}