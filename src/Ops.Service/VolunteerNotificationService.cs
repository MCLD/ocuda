using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Keys.SiteSetting;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Service
{
    public class VolunteerNotificationService
        : BaseService<ScheduleNotificationService>, IVolunteerNotificationService
    {
        private const string EmailDescription = "Volunteer submission notification";
        private const string EmailOverflowDescription = "Volunteer submission overflow notification";
        private const int MaximumEmailsPeriodHours = 1;
        private const int MaximumEmailsPerPeriod = 4;
        private const string NoIntranetLink = "the volunteer section of the Intranet";
        private const string NotificationType = "volunteer";
        private const string VolunteerSubmissionBasePath = "/VolunteerSubmissions/";
        private const string VolunteerSubmissionPath = "/VolunteerSubmissions/Details/";
        private readonly IOcudaCache _cache;
        private readonly IEmailService _emailService;
        private readonly ISiteSettingService _siteSettingService;
        private readonly IVolunteerFormService _volunteerFormService;

        private IDictionary<VolunteerFormType, int> _overflowEmailIds;

        public VolunteerNotificationService(IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ScheduleNotificationService> logger,
            IOcudaCache cache,
            ISiteSettingService siteSettingService,
            IVolunteerFormService volunteerFormService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(siteSettingService);
            ArgumentNullException.ThrowIfNull(volunteerFormService);

            _cache = cache;
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
                    _logger.LogDebug("Found {PendingNotificationCount} pending {NotificationType} notification(s)",
                        pendingNotifications.Count,
                        NotificationType);

                    var emailSetupMapping = await _volunteerFormService
                        .GetFormEmailSetupMappingAsync();

                    foreach (var pending in pendingNotifications)
                    {
                        using (_logger.BeginScope("Handling {NotificationType} notification for id {NotificationId}",
                            NotificationType,
                            pending.Id))
                        {
                            try
                            {
                                var culture = CultureInfo.CurrentCulture;
                                _logger.LogTrace("Found culture: {Culture}", culture.DisplayName);

                                sentNotifications += await SendAsync(pending,
                                    culture.Name,
                                    new Dictionary<string, string> {
                                        { "FormType", pending.VolunteerFormType.ToString() },
                                        { "SubmittedDate", pending.CreatedAt.ToString("g", culture) },
                                        { "SubmissionLink", await GetLinkAsync(pending.Id) }},
                                    emailSetupMapping[pending.VolunteerFormType.Value]);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex,
                                    "Sending pending {NotificationType} notification id {RequestId} failed: {ErrorMessage}",
                                    NotificationType,
                                    pending.Id,
                                    ex.Message);
                            }
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                }
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Uncaught error sending {NotificationType} notifications: {ErrorMessage}",
                    NotificationType,
                    oex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Uncaught critical error sending {NotificationType} notifications: {ErrorMessage}",
                    NotificationType,
                    ex.Message);
            }

            if (sentNotifications > 0)
            {
                _logger.LogInformation("Scheduled task sent {NotificationCount} {NotificationType} email notifications",
                    NotificationType,
                    sentNotifications);
            }

            return sentNotifications;
        }

        private async Task<string> GetLinkAsync(int? pendingId)
        {
            UriBuilder uri = null;

            var baseLink = await _siteSettingService
                .GetSettingStringAsync(UserInterface.BaseIntranetLink);

            if (!string.IsNullOrEmpty(baseLink))
            {
                uri = new UriBuilder(baseLink)
                {
                    Path = pendingId.HasValue
                        ? VolunteerSubmissionPath + pendingId
                        : VolunteerSubmissionBasePath
                };
            }

            return uri != null ? uri.ToString() : NoIntranetLink;
        }

        /// <summary>
        /// Lookup the overflow email id based on a form type, cache lookups.
        /// </summary>
        /// <param name="formType">The form type for overflow ID lookup</param>
        /// <returns>The ID of the overflow email if there is one, null otherwise</returns>
        private async Task<int?> GetOverflowEmailId(VolunteerFormType formType)
        {
            if (_overflowEmailIds?.Any() != true)
            {
                _overflowEmailIds = await _volunteerFormService
                    .GetEmailSetupOverflowMappingAsync();
            }
            if (_overflowEmailIds?.Any() == true
                && _overflowEmailIds.TryGetValue(formType, out int value))
            {
                return value;
            }
            return null;
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

            var regularDetails = await _emailService
                .GetDetailsAsync(emailSetupId, languageName, tagDictionary);

            var mappings = await _volunteerFormService
                .GetFormUserMappingsAsync(request.VolunteerFormId, request.LocationId);

            foreach (var mapping in mappings)
            {
                int? overflowSetupId = null;
                Utility.Email.Details overflowDetails = null;

                var cacheKey = string.Format(CultureInfo.InvariantCulture,
                    Utility.Keys.Cache.OpsVolunteerEmails,
                    mapping.User.Email);

                var userEmailsSent = await _cache.GetIntFromCacheAsync(cacheKey) ?? 0;

                if (userEmailsSent > MaximumEmailsPerPeriod)
                {
                    await _volunteerFormService
                        .NotifiedStaffAsync(request.Id, null, mapping.User.Id);
                    continue;
                }

                if (userEmailsSent == MaximumEmailsPerPeriod)
                {
                    _logger.LogWarning("Staff {Email} has receved {Maximum} volunteer emails recently, sending the overflow email",
                        mapping.User.Email,
                        MaximumEmailsPerPeriod);

                    overflowSetupId = await GetOverflowEmailId(request.VolunteerFormType.Value);
                    if (overflowSetupId.HasValue)
                    {
                        var topLink = await GetLinkAsync(null);
                        tagDictionary["SubmissionLink"] = topLink;

                        overflowDetails = await _emailService
                            .GetDetailsAsync(overflowSetupId.Value, languageName, tagDictionary);
                    }
                    else
                    {
                        _logger.LogWarning("No overflow email configured, sending the regular email");
                    }
                }

                var sentEmailSetup = overflowSetupId ?? emailSetupId;
                var emailDetails = overflowDetails ?? regularDetails;
                var currentEmailDescription = overflowSetupId.HasValue
                    ? EmailOverflowDescription
                    : EmailDescription;

                try
                {
                    emailDetails.ToEmailAddress = mapping.User.Email;
                    emailDetails.ToName = mapping.User.Name;

                    var sentEmail = await _emailService.SendAsync(emailDetails);

                    if (sentEmail != null)
                    {
                        _logger.LogInformation("{EmailDescription} (setup {EmailSetupId}) sent to {EmailTo}",
                            currentEmailDescription,
                            sentEmailSetup,
                            mapping.User.Email);

                        await _volunteerFormService
                            .NotifiedStaffAsync(request.Id, sentEmail.Id, mapping.User.Id);

                        userEmailsSent++;
                        await _cache.SaveToCacheAsync(cacheKey,
                            userEmailsSent,
                            MaximumEmailsPeriodHours);

                        sentEmails++;
                    }
                    else
                    {
                        _logger.LogWarning("{EmailDescription} (setup {EmailSetupId}) failed sending to {EmailTo}",
                            currentEmailDescription,
                            sentEmailSetup,
                            mapping.User.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                        sentEmailSetup,
                        request.Email.Trim(),
                        ex.Message);
                }
            }

            return sentEmails;
        }
    }
}