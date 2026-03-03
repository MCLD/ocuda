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
    public class DigitalLibraryController(ServiceFacades.Controller<DigitalLibraryController> context,
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

        [HttpGet("[action]")]
        public async Task<IActionResult> All()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var emediaViewModel = await GetViewModelAsync(forceReload);

            emediaViewModel.GroupedEmedia.Add(new EmediaGroup
            {
                SortOrder = 1,
                Emedias = await _emediaService.GetEmediaAsync(forceReload)
            });

            PageTitle = "Digital Library";

            return View("Index", emediaViewModel);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var groupedEmedia = await _emediaService.GetGroupedEmediaAsync(forceReload);

            foreach (var group in groupedEmedia)
            {
                if (!string.IsNullOrWhiteSpace(group.Segment?.SegmentText?.Text))
                {
                    group.Segment.SegmentText.Text = FormatForDisplay(group.Segment.SegmentText);
                }

                foreach (var emedia in group.Emedias)
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

            var emediaViewModel = await GetViewModelAsync(forceReload, groupedEmedia);

            PageTitle = "Digital Library";

            return View(emediaViewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Launch(string id)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var emedia = await _emediaService.GetAsync(forceReload, id);

            if (emedia == null)
            {
                return NotFound();
            }

            var requestAddress = HttpContext.Request.Headers["X-Forwarded-For"];
            if (string.IsNullOrEmpty(requestAddress))
            {
                requestAddress = HttpContext.Connection.RemoteIpAddress.ToString();
            }

            _logger.LogInformation("Launching emedia {Name} via {Method} from {Address} ({ConnectionAddress} via {Referer}",
                emedia.Name,
                emedia.IsHttpPost ? "POST" : "GET",
                requestAddress,
                HttpContext.Connection.RemoteIpAddress,
                HttpContext.Request.Headers.Referer);

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
                AllDescription = "A-Z List",
                PopularDescription = "Popular Items",
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

            PageTitle = "Digital Library";

            return View("Index", emediaViewModel);
        }
    }
}