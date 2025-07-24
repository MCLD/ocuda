using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Repositories;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ScheduleNotificationService
        : BaseService<ScheduleNotificationService>, IScheduleNotificationService
    {
        private const string DefaultLanguage = "en-US";
        private const string NotificationType = "volunteer";
        private readonly IEmailService _emailService;
        private readonly IScheduleLogRepository _scheduleLogRepository;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly ISiteSettingService _siteSettingService;

        public ScheduleNotificationService(ILogger<ScheduleNotificationService> logger,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IScheduleLogRepository scheduleLogRepsitory,
            IScheduleRequestService scheduleRequestService,
            ISiteSettingService siteSettingService) : base(logger, httpContextAccessor)
        {
            ArgumentNullException.ThrowIfNull(emailService);
            ArgumentNullException.ThrowIfNull(scheduleLogRepsitory);
            ArgumentNullException.ThrowIfNull(scheduleRequestService);
            ArgumentNullException.ThrowIfNull(siteSettingService);

            _emailService = emailService;
            _scheduleLogRepository = scheduleLogRepsitory;
            _scheduleRequestService = scheduleRequestService;
            _siteSettingService = siteSettingService;
        }

        private enum EmailType
        {
            ScheduleNotification,
            Followup,
            Cancellation
        }

        public Task<EmailRecord> SendCancellationAsync(ScheduleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return SendCancellationInternalAsync(request);
        }

        public Task<bool> SendFollowupAsync(ScheduleRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            return SendFollowupInternalAsync(request);
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
                    = await _scheduleRequestService.GetPendingNotificationsAsync();

                if (pendingNotifications?.Count > 0)
                {
                    _logger.LogDebug("Found {PendingNotificationCount} pending {NotificationType} notification(s)",
                        pendingNotifications.Count,
                        NotificationType);

                    foreach (var pending in pendingNotifications)
                    {
                        using (_logger.BeginScope("Handling {NotifciationType} notification for id {Id}",
                            NotificationType,
                            pending.Id))
                        {
                            try
                            {
                                var lang = pending.Language
                                .Equals("English", StringComparison.OrdinalIgnoreCase)
                                    ? DefaultLanguage
                                    : pending.Language;
                                _logger.LogTrace("Using language: {Language}", pending.Language);

                                var culture = CultureInfo.GetCultureInfo(lang);
                                _logger.LogTrace("Found culture: {Culture}", culture.DisplayName);

                                var sentEmail = await SendAsync(pending,
                                    lang,
                                    new Dictionary<string, string> {
                                        { "ScheduledDate",
                                            pending.RequestedTime.ToString("d", culture) },
                                        { "ScheduledTime",
                                            pending.RequestedTime.ToString("t", culture) },
                                        { "Scheduled", pending.RequestedTime.ToString("g", culture) },
                                        { "Subject", pending.ScheduleRequestSubject.Subject }},
                                    EmailType.ScheduleNotification);

                                if (sentEmail != null)
                                {
                                    sentNotifications++;

                                    await _scheduleRequestService.SetNotificationSentAsync(pending);
                                }
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch all exceptions in jobs so as to not interrupt program flow.")]
        private async Task<EmailRecord> SendAsync(ScheduleRequest request,
            string lang,
            Dictionary<string, string> tagDictionary,
            EmailType emailType)
        {
            int? setupIdLookup = 0;
            string emailDescription = null;

            switch (emailType)
            {
                case EmailType.ScheduleNotification:
                    setupIdLookup = (int)request.ScheduleRequestSubject.RelatedEmailSetupId;
                    emailDescription = "Request confirmation email";
                    break;

                case EmailType.Followup:
                    setupIdLookup = (int)request.ScheduleRequestSubject.FollowupEmailSetupId;
                    emailDescription = "Follow-up email";
                    break;

                case EmailType.Cancellation:
                    setupIdLookup = (int)request.ScheduleRequestSubject.CancellationEmailSetupId;
                    emailDescription = "Cancellation email";
                    break;
            }

            if (setupIdLookup == null)
            {
                return null;
            }

            int setupId = (int)setupIdLookup;

            var emailDetails = await _emailService.GetDetailsAsync(setupId, lang, tagDictionary);

            emailDetails.ToEmailAddress = request.Email.Trim();
            emailDetails.ToName = request.Name?.Trim();

            try
            {
                var sentEmail = await _emailService.SendAsync(emailDetails);

                if (sentEmail != null)
                {
                    _logger.LogInformation("{EmailDescription} (setup {EmailSetupId}) sent to {EmailTo}",
                        emailDescription,
                        setupId,
                        request.Email.Trim());

                    await _scheduleRequestService.SetFollowupSentAsync(request);

                    await _scheduleLogRepository.AddAsync(new ScheduleLog
                    {
                        CreatedAt = request.CreatedAt,
                        Notes = "Request submitted by customer",
                        ScheduleRequestId = request.Id
                    });

                    await _scheduleLogRepository.AddAsync(new ScheduleLog
                    {
                        CreatedAt = DateTime.Now,
                        Notes = $"{emailDescription} sent to {request.Email.Trim()}.",
                        ScheduleRequestId = request.Id,
                        RelatedEmailId = sentEmail?.Id
                    });
                    await _scheduleLogRepository.SaveAsync();

                    return sentEmail;
                }
                else
                {
                    _logger.LogWarning("{EmailDescription} (setup {EmailSetupId}) failed sending to {EmailTo}",
                        emailDescription,
                        setupId,
                        request.Email.Trim());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                    setupId,
                    request.Email.Trim(),
                    ex.Message);
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch all exceptions in jobs so as to not interrupt program flow.")]
        private async Task<EmailRecord> SendCancellationInternalAsync(ScheduleRequest request)
        {
            var lang = request.Language
                .Equals("English", StringComparison.OrdinalIgnoreCase)
                    ? "en-US"
                    : request.Language;

            var culture = CultureInfo.GetCultureInfo(lang);

            try
            {
                return await SendAsync(request,
                    lang,
                    new Dictionary<string, string>
                    {
                        { "Scheduled", request.RequestedTime.ToString(culture) },
                        { "Subject", request.ScheduleRequestSubject.Subject }
                    },
                    EmailType.Cancellation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Sending cancellation notification id {RequestId} failed: {ErrorMessage}",
                    request.Id,
                    ex.Message);
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Catch all exceptions in jobs so as to not interrupt program flow.")]
        private async Task<bool> SendFollowupInternalAsync(ScheduleRequest request)
        {
            var lang = request.Language
                .Equals("English", StringComparison.OrdinalIgnoreCase)
                    ? "en-US"
                    : request.Language;

            var culture = CultureInfo.GetCultureInfo(lang);
            try
            {
                var sentEmail = await SendAsync(request,
                    lang,
                    new Dictionary<string, string>
                    {
                        { "Scheduled", request.RequestedTime.ToString(culture) },
                        { "Subject", request.ScheduleRequestSubject.Subject }
                    },
                    EmailType.Followup);

                if (sentEmail != null)
                {
                    await _scheduleRequestService.SetFollowupSentAsync(request);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Sending followup notification id {RequestId} failed: {ErrorMessage}",
                    request.Id,
                    ex.Message);
            }
            return false;
        }
    }
}