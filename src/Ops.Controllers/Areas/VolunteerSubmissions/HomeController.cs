using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.VolunteerSubmissions.ViewModels;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Extensions;

namespace Ocuda.Ops.Controllers.Areas.VolunteerSubmissions
{
    [Area(nameof(VolunteerSubmissions))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private const string PageTitle = "Volunteer Submissions";
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILocationService _locationService;
        private readonly IVolunteerFormService _volunteerFormService;

        public HomeController(Controller<HomeController> context,
            IDateTimeProvider dateTimeProvider,
            ILocationService locationService,
            IVolunteerFormService volunteerFormService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(volunteerFormService);

            _dateTimeProvider = dateTimeProvider;
            _locationService = locationService;
            _volunteerFormService = volunteerFormService;

            SetPageTitle(PageTitle);
        }

        public static string Name
        { get { return "Home"; } }

        [HttpGet("")]
        [HttpGet("[action]")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> All(int page, int selectedLocation)
        {
            int currentPage = page != 0 ? page : 1;

            var filter = new VolunteerSubmissionFilter(currentPage)
            {
                SelectedLocation = selectedLocation
            };

            var viewModel = await GetSubmissionsAsync(filter);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.SecondaryHeading = nameof(All);
            viewModel.SelectedLocation = selectedLocation;

            return View("Index", viewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Details(int id, int page, int selectedLocation)
        {
            var submission = await _volunteerFormService.GetVolunteerFormSubmissionAsync(id);

            var viewModel = new DetailsViewModel
            {
                BackLink = (page, selectedLocation) switch
                {
                    ( > 0, > 0) => Url.Action(nameof(All), new { page, selectedLocation }),
                    ( > 0, _) => Url.Action(nameof(All), new { page }),
                    (_, > 0) => Url.Action(nameof(All), new { selectedLocation }),
                    _ => Url.Action(nameof(All))
                },
                CurrentPage = page,
                VolunteerFormSubmission = submission,
                SecondaryHeading = submission?.Name,
                SelectedLocation = selectedLocation,
            };

            if (submission != null)
            {
                viewModel.VolunteerFormHistory.Add(new VolunteerFormHistory
                {
                    Text = "Form submitted",
                    Timestamp = submission.CreatedAt
                });

                if (submission.StaffNotifiedAt.HasValue)
                {
                    viewModel.VolunteerFormHistory.Add(new VolunteerFormHistory
                    {
                        Text = "Form marked as staff notified",
                        Timestamp = submission.StaffNotifiedAt.Value
                    });
                }

                var emailRecords = await _volunteerFormService
                    .GetNotificationInfoAsync(submission.Id);

                if (emailRecords?.Count > 0)
                {
                    foreach (var record in emailRecords)
                    {
                        viewModel.VolunteerFormHistory.Add(new VolunteerFormHistory
                        {
                            Text = $"Sent email to {record.EmailRecord.ToEmailAddress}",
                            Timestamp = record.EmailRecord.CreatedAt,
                            User = record.User
                        });
                    }
                }
            }

            SetPageTitle("Volunteer Submission");

            return View("Details", viewModel);
        }

        private async Task<IndexViewModel> GetSubmissionsAsync(VolunteerSubmissionFilter filter)
        {
            var submissions = await _volunteerFormService.GetPaginatedSubmissionsAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = filter.Page,
                ItemCount = submissions.Count,
                ItemsPerPage = filter.Take.Value
            };
            viewModel.AllLocationNames
                .AddRange(await _locationService.GetAllNamesIncludingDeletedAsync());
            viewModel.Submissions.AddRange(submissions.Data);
            viewModel.SubmissionTypes
                .AddRange(_volunteerFormService.GetAllVolunteerFormTypes());

            return viewModel;
        }
    }
}