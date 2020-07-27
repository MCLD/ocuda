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
        private readonly IEmailService _emailService;
        private readonly IScheduleLogRepository _scheduleLogRepository;
        private readonly IScheduleRequestService _scheduleRequestService;
        private readonly ISiteSettingService _siteSettingService;

        private enum EmailType
        {
            ScheduleNotification,
            Followup,
            Cancellation
        }

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

        public async Task<Configuration> GetEmailSettingsAsync()
        {
            var config = new Configuration
            {
                BccAddress = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.BccAddress),
                FromAddress = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.FromAddress),
                FromName = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.FromName),
                OutgoingHost = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingHost),
                OutgoingLogin = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingLogin),
                OutgoingPassword = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingPassword),
                OutgoingPort = await _siteSettingService
                    .GetSettingIntAsync(Ops.Models.Keys.SiteSetting.Email.OutgoingPort),
                OverrideToAddress = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.OverrideToAddress),
                RestrictToDomain = await _siteSettingService
                    .GetSettingStringAsync(Ops.Models.Keys.SiteSetting.Email.RestrictToDomain)
            };

            if (string.IsNullOrEmpty(config.FromAddress))
            {
                throw new OcudaEmailException("From address is not configured");
            }

            if (string.IsNullOrEmpty(config.FromName))
            {
                throw new OcudaEmailException("From name is not configured");
            }

            if (string.IsNullOrEmpty(config.OutgoingHost))
            {
                throw new OcudaEmailException("Outgoing mail host is not configured");
            }

            return config;
        }

        public async Task<int> SendPendingNotificationsAsync()
        {
            int sentNotifications = 0;
            var pendingNotifications
                = await _scheduleRequestService.GetPendingNotificationsAsync();

            if (pendingNotifications?.Count > 0)
            {
                _logger.LogDebug("Found {PendingNotificationCount} pending notification(s)",
                    pendingNotifications.Count);

                Configuration settings = null;
                try
                {
                    settings = await GetEmailSettingsAsync();
                }
                catch (OcudaEmailException) { }

                if (settings != null)
                {
                    foreach (var pending in pendingNotifications)
                    {
                        var lang = pending.Language
                            .Equals("English", StringComparison.OrdinalIgnoreCase)
                                ? "en-US"
                                : pending.Language;

                        var culture = CultureInfo.GetCultureInfo(lang);

                        try
                        {
                            var sentEmail = await SendAsync(pending,
                                settings,
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

            return sentNotifications;
        }

        public async Task SendFollowupAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Configuration settings = null;
            try
            {
                settings = await GetEmailSettingsAsync();
            }
            catch (OcudaEmailException) { }

            if (settings != null)
            {
                var lang = request.Language
                    .Equals("English", StringComparison.OrdinalIgnoreCase)
                        ? "en-US"
                        : request.Language;

                var culture = CultureInfo.GetCultureInfo(lang);
                try
                {
                    var sentEmail = await SendAsync(request,
                        settings,
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
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Sending followup notification id {RequestId} failed: {ErrorMessage}",
                        request.Id,
                        ex.Message);
                }
            }
        }

        public async Task<EmailRecord> SendCancellationAsync(ScheduleRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            Configuration settings = null;
            try
            {
                settings = await GetEmailSettingsAsync();
            }
            catch (OcudaEmailException) { }

            if (settings != null)
            {
                var lang = request.Language
                    .Equals("English", StringComparison.OrdinalIgnoreCase)
                        ? "en-US"
                        : request.Language;

                var culture = CultureInfo.GetCultureInfo(lang);

                try
                {
                    return await SendAsync(request,
                        settings,
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
            }
            return null;
        }

        private async Task<EmailRecord> SendAsync(ScheduleRequest request,
            Configuration settings,
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

            var emailSetupText = await _emailService.GetEmailSetupAsync(setupId, lang);

            var emailTemplateText = await _emailService
                .GetEmailTemplateAsync(emailSetupText.EmailSetup.EmailTemplateId, lang);

            var emailDetails = new Details
            {
                FromEmailAddress = emailSetupText.EmailSetup.FromEmailAddress,
                FromName = emailSetupText.EmailSetup.FromName,
                ToEmailAddress = request.Email.Trim(),
                ToName = request.Name?.Trim(),
                UrlParameters = emailSetupText.UrlParameters,
                Preview = emailSetupText.Preview,
                TemplateHtml = emailTemplateText.TemplateHtml,
                TemplateText = emailTemplateText.TemplateText,
                BodyText = emailSetupText.BodyText,
                BodyHtml = emailSetupText.BodyHtml,
                Tags = tagDictionary,
                BccEmailAddress = settings.BccAddress,
                OverrideEmailToAddress = settings.OverrideToAddress,
                Password = settings.OutgoingPassword,
                Port = settings.OutgoingPort,
                RestrictToDomain = settings.RestrictToDomain,
                Server = settings.OutgoingHost,
                Subject = emailSetupText.Subject,
                Username = settings.OutgoingLogin
            };

            try
            {
                _logger.LogDebug("{EmailDescription} (setup {EmailSetupId}) sending to {EmailTo}",
                    emailDescription,
                    emailSetupText.EmailSetup.Id,
                    request.Email.Trim());

                var sentEmail = await _emailService.SendAsync(emailDetails);

                _logger.LogInformation("{EmailDescription} (setup {EmailSetupId}) sent to {EmailTo}",
                    emailDescription,
                    emailSetupText.EmailSetup.Id,
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
            catch (Exception ex)
            {
                _logger.LogError("Error sending email setup {EmailSetupId} to {EmailTo}: {ErrorMessage}",
                    emailSetupText.EmailSetup.Id,
                    request.Email.Trim(),
                    ex.Message);
            }

            return null;
        }
    }
}
