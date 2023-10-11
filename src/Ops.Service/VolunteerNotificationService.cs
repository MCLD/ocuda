using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Keys.SiteSetting;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class VolunteerNotificationService : BaseService<ScheduleNotificationService>,
        IVolunteerNotificationService
    {
        private const string EmailDescription = "Volunteer submission notification";
        private const string NoIntranetLink = "the volunteer section of the Intranet";
        private const string VolunteerSubmissionPath = "/VolunteerSubmissions/Details/";

        private readonly IEmailService _emailService;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IVolunteerFormService _volunteerFormService;

        public VolunteerNotificationService(IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ScheduleNotificationService> logger,
            ISiteSettingService siteSettingService,
            IVolunteerFormService volunteerFormService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(siteSettingService);
            ArgumentNullException.ThrowIfNull(volunteerFormService);

            _emailService = emailService;
            _siteSettingService = siteSettingService;
            _volunteerFormService = volunteerFormService;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch all exceptions in jobs so as to not interrupt program flow.")]
        public async Task<int> SendPendingNotificationsAsync()
        {
            int sentNotifications = 0;
            try
            {
                var pendingNotifications
                    = await _volunteerFormService.GetPendingNotificationsAsync();

                if (pendingNotifications?.Count > 0)
                {
                    _logger.LogDebug("Found {PendingNotificationCount} pending notification(s)",
                        pendingNotifications.Count);

                    var baseLink = await _siteSettingService
                        .GetSettingStringAsync(UserInterface.BaseIntranetLink);

                    var emailSetupMapping = await _volunteerFormService
                        .GetFormEmailSetupMappingAsync();

                    foreach (var pending in pendingNotifications)
                    {
                        try
                        {
                            var culture = CultureInfo.CurrentCulture;
                            _logger.LogTrace("Found culture: {Culture}", culture.DisplayName);

                            UriBuilder uri = null;
                            if (!string.IsNullOrEmpty(baseLink))
                            {
                                uri = new UriBuilder(baseLink)
                                {
                                    Path = VolunteerSubmissionPath + pending.Id
                                };
                            }

                            sentNotifications += await SendAsync(pending,
                                    culture.Name,
                                    new Dictionary<string, string>
                                    {
                                    { "FormType", pending.VolunteerFormType.ToString() },
                                    { "SubmittedDate", pending.CreatedAt.ToString("g", culture) },
                                    { "SubmissionLink", uri != null
                                        ? uri.ToString()
                                        : NoIntranetLink }
                                    },
                                    emailSetupMapping[pending.VolunteerFormType.Value]);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("Sending pending notification id {RequestId} failed: {ErrorMessage}",
                                pending.Id,
                                ex.Message);
                        }

                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Uncaught error sending notifications: {ErrorMessage}",
                    oex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Uncaught critical error sending notifications: {ErrorMessage}",
                    ex.Message);
            }

            if (sentNotifications > 0)
            {
                _logger.LogInformation("Scheduled task sent {NotificationCount} email notifications",
                    sentNotifications);
            }

            return sentNotifications;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Any email failure should not interrupt emailing other staff members.")]
        private async Task<int> SendAsync(VolunteerFormSubmission request,
            string languageName,
            Dictionary<string, string> tagDictionary,
            int emailSetupId)
        {
            int sentEmails = 0;

            var emailDetails = await _emailService
                .GetDetailsAsync(emailSetupId, languageName, tagDictionary);

            var mappings = await _volunteerFormService
                .GetFormUserMappingsAsync(request.VolunteerFormId, request.LocationId);

            foreach (var mapping in mappings)
            {
                try
                {
                    emailDetails.ToEmailAddress = mapping.User.Email;
                    emailDetails.ToName = mapping.User.Name;

                    var sentEmail = await _emailService.SendAsync(emailDetails);

                    if (sentEmail != null)
                    {
                        _logger.LogInformation("{EmailDescription} (setup {EmailSetupId}) sent to {EmailTo}",
                            EmailDescription,
                            emailSetupId,
                            mapping.User.Email);

                        await _volunteerFormService
                            .NotifiedStaffAsync(request.Id, sentEmail.Id, mapping.User.Id);

                        sentEmails++;
                    }
                    else
                    {
                        _logger.LogWarning("{EmailDescription} (setup {EmailSetupId}) failed sending to {EmailTo}",
                            EmailDescription,
                            emailSetupId,
                            mapping.User.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                        emailSetupId,
                        request.Email.Trim(),
                        ex.Message);
                }
            }

            return sentEmails;
        }
    }
}