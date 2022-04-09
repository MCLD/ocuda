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
using Ocuda.Utility.Email;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ScheduleNotificationService
        : BaseService<ScheduleNotificationService>, IScheduleNotificationService
    {
        private const string DefaultLanguage = "en-US";
        private readonly IEmailService _emailService;
        private readonly IScheduleLogRepository _scheduleLogRepository;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly ISiteSettingService _siteSettingService;

        public ScheduleNotificationService(ILogger<ScheduleNotificationService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IScheduleLogRepository scheduleLogRepsitory,
            IScheduleRequestService scheduleRequestService,
            ISiteSettingService siteSettingService) : base(logger, httpContextAccessor)
        {
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _scheduleLogRepository = scheduleLogRepsitory
                ?? throw new ArgumentNullException(nameof(scheduleLogRepsitory));
            _scheduleRequestService = scheduleRequestService
                ?? throw new ArgumentNullException(nameof(scheduleRequestService));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        private enum EmailType
        {
            ScheduleNotification,
            Followup,
            Cancellation
        }

        public Task<EmailRecord> SendCancellationAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return SendCancellationInternalAsync(request);
        }

        public Task<bool> SendFollowupAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            return SendFollowupInternalAsync(request);
        }

        public async Task<int> SendPendingNotificationsAsync()
        {
            int sentNotifications = 0;

            try
            {
                var pendingNotifications
                    = await _scheduleRequestService.GetPendingNotificationsAsync();

                if (pendingNotifications?.Count > 0)
                {
                    _logger.LogDebug("Found {PendingNotificationCount} pending notification(s)",
                        pendingNotifications.Count);

                    foreach (var pending in pendingNotifications)
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
                                new Dictionary<string, string>
                                {
                                    { "ScheduledDate",
                                        pending.RequestedTime.ToString("d", culture) },
                                    { "ScheduledTime",
                                        pending.RequestedTime.ToString("t", culture) },
                                    { "Scheduled", pending.RequestedTime.ToString("g", culture) },
                                    { "Subject", pending.ScheduleRequestSubject.Subject }
                                },
                                EmailType.ScheduleNotification);

                            if (sentEmail != null)
                            {
                                sentNotifications++;

                                await _scheduleRequestService.SetNotificationSentAsync(pending);
                            }
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
                _logger.LogError("Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                    setupId,
                    request.Email.Trim(),
                    ex.Message);
            }

            return null;
        }

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
                _logger.LogError("Sending cancellation notification id {RequestId} failed: {ErrorMessage}",
                    request.Id,
                    ex.Message);
            }

            return null;
        }

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
                _logger.LogError("Sending followup notification id {RequestId} failed: {ErrorMessage}",
                    request.Id,
                    ex.Message);
            }
            return false;
        }
    }
}
